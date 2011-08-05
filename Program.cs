using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using HL7TestClient.Constants;
using HL7TestClient.Interfaces;
using HL7TestClient.PersonRegistry;
using NHN.HL7.Constants;

namespace HL7TestClient
{
    class Program
    {
        private const string FhNumberOid = "2.16.578.1.12.4.1.4.3";
        private const string DateFormat = "yyyyMMdd";
        
        private const string RootNamespace = "urn:hl7-org:v3";
        private static readonly XmlSerializer FindCandidatesRequestSerializer = new XmlSerializer(typeof (PRPA_IN101305NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer FindCandidatesResponseSerializer = new XmlSerializer(typeof (PRPA_IN101306NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer GetDemographicsRequestSerializer = new XmlSerializer(typeof (PRPA_IN101307NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer GetDemographicsResponseSerializer = new XmlSerializer(typeof (PRPA_IN101308NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer AddPersonRequestSerializer = new XmlSerializer(typeof (PRPA_IN101311NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer RevisePersonRecordRequestSerializer = new XmlSerializer(typeof (PRPA_IN101314NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer AddPersonOrRevisePersonRecordResponseSerializer = new XmlSerializer(typeof (PRPA_IN101319NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer LinkPersonRecordsRequestSerializer = new XmlSerializer(typeof (PRPA_IN101901NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer UnlinkPersonRecordsRequestSerializer = new XmlSerializer(typeof (PRPA_IN101911NO01), new XmlRootAttribute {Namespace = RootNamespace});
        private static readonly XmlSerializer AcknowledgementSerializer = new XmlSerializer(typeof (MCAI_IN000004NO01), new XmlRootAttribute {Namespace = RootNamespace});

        private static CS _processingCode = ProcessingCode.Test();
        
        private static readonly Random Random = new Random();

        static void Main()
        {
            var client = CreateClient();
            while (true)
            {
                try
                {
                    Console.Write("\nWould you like to (F)indCandidates, (G)etDemographics, (A)ddPerson, (R)evisePersonRecord, (L)inkPersonRecords, (U)nlinkPersonRecords, change (P)rocessing code, run performance (T)est, or (E)xit? ");
                    string action = (Console.ReadLine() ?? "E").Trim().ToUpper();
                    if (action == "F")
                        FindCandidates(client);
                    else if (action == "G")
                        GetDemographics(client);
                    else if (action == "A")
                        AddPerson(client);
                    else if (action == "R")
                        RevisePersonRecord(client);
                    else if (action == "L")
                        LinkPersonRecords(client);
                    else if (action == "U")
                        UnlinkPersonRecords(client);
                    else if (action == "P")
                        ChangeProcessingCode();
                    else if (action == "T")
                        RunPerformanceTest(client);
                    else if (action == "E")
                        break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    if (e is EndpointNotFoundException && e.InnerException is WebException)
                    {
                        if (e.InnerException.Message == "The remote server returned an error: (404) Not Found.")
                            Console.WriteLine("\n\n=== An error occurred when connecting to the server: the server was found, but the service endpoint was not found. This probably means that the URL in the config file is wrong. ===\n");
                        else if (e.InnerException.Message.StartsWith("The remote name could not be resolved: "))
                            Console.WriteLine("\n\n=== An error occurred when connecting to the server: the server was not found. This probably means that the URL in the config file is wrong. ===\n");
                    }
                    else if (e is MessageSecurityException && e.InnerException is FaultException)
                    {
                        if (e.InnerException.Message == "An error occurred when verifying security for the message.")
                            Console.WriteLine("\n\n=== An error occurred when establishing a secure connection. A possible reason for this is that your local machine clock is out of synch with NHN's server clock. Please contact NHN and ask what the server time is on the server you're trying to connect to. ===\n");
                        else if (e.InnerException.Message == "At least one security token in the message could not be validated.")
                            Console.WriteLine("\n\n=== An error occurred when authenticating. This probably means that the username and/or password in the config file are wrong. ===\n");
                    }

                    client.Abort();
                    ((IDisposable) client).Dispose();
                    client = CreateClient();
                }
            }
            client.Abort();
            ((IDisposable) client).Dispose();
        }

        private static PersonRegistryClient CreateClient()
        {
            var client = new PersonRegistryClient();
            client.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["username"];
            client.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["password"];
            return client;
        }

        private static void FindCandidates(PersonRegistryClient client)
        {
            var info = ReadPersonalInformation(false, false);
            var paramList = new PRPA_MT101306NO01ParameterList();

            var nameItems = CreateNameItems(info);
            if (nameItems.Count() > 0)
                paramList.personName = CreatePersonNameParameter(nameItems);

            if (IsDateSpecifiedAndValid(info.DateOfBirth))
                paramList.personBirthTime = CreatePersonBirthTimeParameter(info.DateOfBirth);

            var addressItems = CreateAddressItems(info);
            if (addressItems.Count() > 0)
                paramList.identifiedPersonAddress = CreateIdentifiedPersonAddressParameter(addressItems);

            var message = SetTopLevelFields(new PRPA_IN101305NO01 {
                controlActProcess = new PRPA_IN101305NO01QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101306NO01QueryByParameter {
                        parameterList = paramList
                    }
                }
            });

            FindCandidatesRequestSerializer.Serialize(Console.Out, message);
            Console.WriteLine("\n");
            PRPA_IN101306NO01 result = client.FindCandidates(message);
            FindCandidatesResponseSerializer.Serialize(Console.Out, result);
            Console.WriteLine("\n");

            Console.WriteLine("Found {0} persons:", result.controlActProcess.queryAck.resultTotalQuantity.value);
            if (result.controlActProcess.subject != null)
                foreach (var subject in result.controlActProcess.subject)
                    Console.WriteLine(PersonToString(subject.registrationEvent.subject1.identifiedPerson));
        }

        private static void GetDemographics(PersonRegistryClient client)
        {
            string idNumber = ReadLineAndTrim("Enter id number: ");
            if (string.IsNullOrWhiteSpace(idNumber))
                return;

            var id = new II {root = GetOid(idNumber.Trim()), extension = idNumber.Trim()};
            PRPA_IN101307NO01 request = CreateGetDemographicsRequest(id);

            GetDemographicsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            PRPA_IN101308NO01 response = client.GetDemographics(request);
            GetDemographicsResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine("\n");

            string queryResponseCode = response.controlActProcess.queryAck.queryResponseCode.code;
            switch (queryResponseCode)
            {
                case QueryResponseCode.Ok:
                    Console.WriteLine(PersonToString(response.controlActProcess.subject[0].registrationEvent.subject1.identifiedPerson));
                    break;
                case QueryResponseCode.NoResultsFound:
                    Console.WriteLine("No results found");
                    break;
                case QueryResponseCode.QueryParameterError:
                    Console.WriteLine("Query parameter error");
                    break;
                default:
                    Console.WriteLine("Unrecognized query response code: '{0}'", queryResponseCode);
                    break;
            }
        }

        private static PRPA_IN101307NO01 CreateGetDemographicsRequest(II id)
        {
            return SetTopLevelFields(new PRPA_IN101307NO01 {
                controlActProcess = new PRPA_IN101307NO01QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101307UV02QueryByParameter {
                        parameterList = new PRPA_MT101307UV02ParameterList {
                            identifiedPersonIdentifier = new[] {
                                new PRPA_MT101307UV02IdentifiedPersonIdentifier {value = new[] {id}}
                            }
                        }
                    }
                }
            });
        }

        private static void AddPerson(PersonRegistryClient client)
        {
            var info = ReadPersonalInformation(false, true);

            var innerPerson = new PRPA_MT101311NO01Person();

            var nameItems = CreateNameItems(info);
            if (nameItems.Count() > 0)
                innerPerson.name = new[] {new PN(nameItems)};

            if (IsDateSpecifiedAndValid(info.DateOfBirth))
                innerPerson.birthTime = new TS(info.DateOfBirth);

            var addressItems = CreateAddressItems(info);
            if (addressItems.Count() > 0)
                innerPerson.addr = new[] {new AD(addressItems)};

            if (info.Gender != "")
                innerPerson.administrativeGenderCode = CreateAdministrativeGenderCode(info.Gender);

            var request = SetTopLevelFields(new PRPA_IN101311NO01 {
                controlActProcess = new PRPA_IN101311NO01MFMI_MT700721UV01ControlActProcess {
                    subject = new PRPA_IN101311NO01MFMI_MT700721UV01Subject1 {
                        registrationRequest = new PRPA_IN101311NO01MFMI_MT700721UV01RegistrationRequest {
                            subject1 = new PRPA_IN101311NO01MFMI_MT700721UV01Subject2 {
                                identifiedPerson = new PRPA_MT101311NO01IdentifiedPerson {
                                    identifiedPerson = innerPerson
                                }
                            }
                        }
                    }
                }
            });
            
            AddPersonRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            PRPA_IN101319NO01 response = client.AddPerson(request);
            AddPersonOrRevisePersonRecordResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine("\n");

            var pathToFirstNull = new List<string>();
            var subject = NullSafeObjectPathTraverser.Traverse(response, r => r.controlActProcess.subject, pathToFirstNull);
            if (subject != null && subject.Length > 0)
            {
                var id = NullSafeObjectPathTraverser.Traverse(subject[0], s => s.registrationEvent.subject1.identifiedPerson.id, pathToFirstNull);
                if (id != null && id.Length > 0)
                    Console.WriteLine("The person has been given the FH-number " + id[0].extension);
            }
        }

        private static CE CreateAdministrativeGenderCode(string gender)
        {
            return new CE {code = gender, codeSystem = "2.16.840.1.113883.5.1"};
        }

        private static void RevisePersonRecord(PersonRegistryClient client)
        {
            var info = ReadPersonalInformation(true, true);
            
            var outerPerson = new PRPA_MT101302NO01IdentifiedPerson {
                id = new[] {new II {root = GetOid(info.FhNumber), extension = info.FhNumber}},
                identifiedPerson = new PRPA_MT101302NO01Person()
            };

            var nameItems = CreateNameItems(info);
            if (nameItems.Count() > 0)
                outerPerson.identifiedPerson.name = new[] {new PN(nameItems)};

            if (IsDateSpecifiedAndValid(info.DateOfBirth))
                outerPerson.identifiedPerson.birthTime = new TS(info.DateOfBirth);

            var addressItems = CreateAddressItems(info);
            if (addressItems.Count() > 0)
                outerPerson.identifiedPerson.addr = new[] {new AD(addressItems)};
            
            if (info.Gender != "")
                outerPerson.identifiedPerson.administrativeGenderCode = CreateAdministrativeGenderCode(info.Gender);

            var request = SetTopLevelFields(new PRPA_IN101314NO01 {
                controlActProcess = new PRPA_IN101314NO01MFMI_MT700721UV01ControlActProcess {
                    subject = new PRPA_IN101314NO01MFMI_MT700721UV01Subject1 {
                        registrationRequest = new PRPA_IN101314NO01MFMI_MT700721UV01RegistrationRequest {
                            subject1 = new PRPA_IN101314NO01MFMI_MT700721UV01Subject2 {
                                identifiedPerson = outerPerson
                            }
                        }
                    }
                }
            });

            RevisePersonRecordRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            PRPA_IN101319NO01 response = client.RevisePersonRecord(request);
            AddPersonOrRevisePersonRecordResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }

        private static void LinkPersonRecords(PersonRegistryClient client)
        {
            Console.Write("Enter obsolete FH-number: ");
            string obsoleteFhNumber = Console.ReadLine();
            Console.Write("Enter surviving ID-number or FH-number: ");
            string survivingIdNumberOrFhNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(obsoleteFhNumber) || string.IsNullOrWhiteSpace(survivingIdNumberOrFhNumber))
                return;

            var request = SetTopLevelFields(new PRPA_IN101901NO01 {
                controlActProcess = new PRPA_IN101901NO01MFMI_MT700721UV01ControlActProcess {
                    subject = new PRPA_IN101901NO01MFMI_MT700721UV01Subject1 {
                        registrationRequest = new PRPA_IN101901NO01MFMI_MT700721UV01RegistrationRequest {
                            subject1 = new PRPA_IN101901NO01MFMI_MT700721UV01Subject2 {
                                identifiedPerson = new PRPA_MT101901NO01IdentifiedPerson {
                                    id = new[] {new II(GetOid(survivingIdNumberOrFhNumber), survivingIdNumberOrFhNumber)},
                                    identifiedBy = new[] {
                                        new PRPA_MT101901NO01SourceOf2 {
                                            //TODO: Is the value of statusCode important?
                                            otherIdentifiedPerson = new PRPA_MT101901NO01OtherIdentifiedPerson {
                                                id = new II(GetOid(obsoleteFhNumber), obsoleteFhNumber)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            
            LinkPersonRecordsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            MCAI_IN000004NO01 response = client.LinkPersonRecords(request);
            AcknowledgementSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }

        private static void UnlinkPersonRecords(PersonRegistryClient client)
        {
            Console.Write("Enter child FH-number: ");
            string obsoleteFhNumber = Console.ReadLine();
            Console.Write("Enter parent ID-number or FH-number: ");
            string survivingIdNumberOrFhNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(obsoleteFhNumber) || string.IsNullOrWhiteSpace(survivingIdNumberOrFhNumber))
                return;

            var request = SetTopLevelFields(new PRPA_IN101911NO01 {
                controlActProcess = new PRPA_IN101911NO01MFMI_MT700721UV01ControlActProcess {
                    subject = new PRPA_IN101911NO01MFMI_MT700721UV01Subject1 {
                        registrationRequest = new PRPA_IN101911NO01MFMI_MT700721UV01RegistrationRequest {
                            subject1 = new PRPA_IN101911NO01MFMI_MT700721UV01Subject2 {
                                identifiedPerson = new PRPA_MT101911NO01IdentifiedPerson {
                                    id = new[] {new II(GetOid(survivingIdNumberOrFhNumber), survivingIdNumberOrFhNumber)},
                                    identifiedBy = new PRPA_MT101911NO01SourceOf2 {
                                        //TODO: Is the value of statusCode important?
                                        otherIdentifiedPerson = new PRPA_MT101911NO01OtherIdentifiedPerson {
                                            id = new II(GetOid(obsoleteFhNumber), obsoleteFhNumber)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            
            UnlinkPersonRecordsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine("\n");
            MCAI_IN000004NO01 response = client.UnlinkPersonRecords(request);
            AcknowledgementSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }

        private static void ChangeProcessingCode()
        {
            Console.Write("Please choose (P)roduction, (T)est, or (D)ebugging: ");
            while (true)
            {
                string code = (Console.ReadLine() ?? "").ToUpper();
                if (code == "P")
                {
                    _processingCode = ProcessingCode.Production();
                    return;
                }
                if (code == "T")
                {
                    _processingCode = ProcessingCode.Test();
                    break;
                }
                if (code == "D")
                {
                    _processingCode = ProcessingCode.Debugging();
                    break;
                }
            }
        }

        private static void RunPerformanceTest(PersonRegistryClient client)
        {
            var sw = new Stopwatch();

            sw.Start();
            client.GetDemographics(CreateGetDemographicsRequest(new II(FhNumberOid, CreateRandomFhNumber())));
            sw.Stop();
            Console.WriteLine("Initial request: " + sw.Elapsed);

            sw.Restart();
            const int numRequests = 1000;
            for (int i = 0; i < numRequests; ++i)
            {
                client.GetDemographics(CreateGetDemographicsRequest(new II(FhNumberOid, CreateRandomFhNumber())));
            }
            sw.Stop();
            Console.WriteLine("{0} subsequent requests: {1} ({2} ms per request)", numRequests, sw.Elapsed, sw.ElapsedMilliseconds / numRequests);

            sw.Restart();
            const int numIndividualRequests = 100;
            for (int i = 0; i < numIndividualRequests; ++i)
            {
                using (var c = CreateClient())
                {
                    c.GetDemographics(CreateGetDemographicsRequest(new II(FhNumberOid, CreateRandomFhNumber())));
                }
            }
            sw.Stop();
            Console.WriteLine("{0} individual requests: {1} ({2} ms per request)", numIndividualRequests, sw.Elapsed, sw.ElapsedMilliseconds / numIndividualRequests);

            const int numThreads = 20;
            var threads = new Thread[numThreads];
            for (int i = 0; i < numThreads; ++i)
            {
                threads[i] = new Thread(ThreadTester);
                threads[i].Start(i);
            }
            foreach (var thread in threads)
                thread.Join();
        }

        private static void ThreadTester(object threadId)
        {
            var sw = new Stopwatch();
            sw.Start();
            const int numIndividualRequests = 50;
            using (var c = CreateClient())
            {
                for (int i = 0; i < numIndividualRequests; ++i)
                {
                    c.GetDemographics(CreateGetDemographicsRequest(new II(FhNumberOid, CreateRandomFhNumber())));
                }
            }
            sw.Stop();
            Console.WriteLine("{0} individual requests from thread #{1}: {2} ({3} ms per request)", numIndividualRequests, (int)threadId, sw.Elapsed, sw.ElapsedMilliseconds / numIndividualRequests);
        }

        private static string CreateRandomFhNumber()
        {
            while (true)
            {
                string fhNumber = Random.Next(800000000, 800009999).ToString();
                if (ChecksumGenerator.AppendChecksum(ref fhNumber))
                    return fhNumber;
            }
        }

        private static IEnumerable<ENXP> CreateNameItems(PersonalInformation info)
        {
            var nameItems = new List<ENXP>();
            nameItems.AddRange(info.FirstName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(fn => new engiven(fn)));
            nameItems.AddRange(info.MiddleName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(mn => new enfamily(mn, true)));
            nameItems.AddRange(info.LastName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(ln => new enfamily(ln)));
            return nameItems;
        }

        private static IEnumerable<ADXP> CreateAddressItems(PersonalInformation info)
        {
            var addressItems = new List<ADXP>();
            if (info.StreetAddressLine != "")
                addressItems.Add(new adxpstreetAddressLine {Text = new[] {info.StreetAddressLine}});
            if (info.ZipCode != "")
                addressItems.Add(new adxppostalCode {Text = new[] {info.ZipCode}});
            return addressItems;
        }

        private static bool IsDateSpecifiedAndValid(string date)
        {
            if (date == "")
                return false;

            DateTime dummy;
            if (DateTime.TryParseExact(date, DateFormat, null, DateTimeStyles.None, out dummy))
                return true;

            Console.WriteLine("Warning: Date of birth is illegal; skipping");
            return false;
        }

        private static PRPA_MT101306NO01PersonName[] CreatePersonNameParameter(IEnumerable<ENXP> nameItems)
        {
            return new[] {new PRPA_MT101306NO01PersonName {value = new[] {new PN {Items = nameItems.ToArray()}}}};
        }
        
        private static PRPA_MT101306NO01IdentifiedPersonAddress[] CreateIdentifiedPersonAddressParameter(IEnumerable<ADXP> addressItems)
        {
            return new[] {new PRPA_MT101306NO01IdentifiedPersonAddress {value = new[] {new AD {Items = addressItems.ToArray()}}}};
        }
        
        private static PRPA_MT101306NO01PersonBirthTime[] CreatePersonBirthTimeParameter(string dateOfBirth)
        {
            return new[] {new PRPA_MT101306NO01PersonBirthTime {value = new[] {new IVL_TS {value = dateOfBirth}}}};
        }

        private static string GetOid(string idNumber)
        {
            if (idNumber[0] >= '8')
                return FhNumberOid;
            else if (idNumber[0] >= '4')
                return "2.16.578.1.12.4.1.4.2";
            else
                return "2.16.578.1.12.4.1.4.1";
        }

        private static II CreateMessageId()
        {
            return new II("1.2.3.4", Guid.NewGuid().ToString());
        }

        private static string PersonToString(IIdentifiedPerson identifiedPerson)
        {
            var sb = new StringBuilder();
            sb.Append(identifiedPerson.id[0].extension);
            sb.Append(": ");

            IPerson person = identifiedPerson.identifiedPerson;

            if (person.name != null && person.name.Length > 0 && person.name[0].Items != null)
                sb.Append(string.Join(" ", person.name[0].Items.Select(ni => ni.Text[0])));
            else
                sb.Append("(no name)");

            if (person.administrativeGenderCode != null)
            {
                sb.Append("; Gender: ");
                sb.Append(person.administrativeGenderCode.code);
            }

            if (person.birthTime != null)
            {
                sb.Append("; Date of birth: ");
                sb.Append(person.birthTime.value);
            }

            if (person.addr != null && person.addr.Length > 0)
            {
                sb.Append("; Address: ");
                sb.Append(string.Join(" ", person.addr[0].Items.Select(ai => ai.Text != null && ai.Text.Length > 0 ? ai.Text[0] : "")));
            }

            return sb.ToString();
        }

        private static TMessage SetTopLevelFields<TMessage>(TMessage message)
            where TMessage : IRequestMessage
        {
            message.id = CreateMessageId();
            message.interactionId = new II("2.16.840.1.113883.1.6", typeof (TMessage).Name);
            message.processingCode = _processingCode;
            message.processingModeCode = new CS("T");
            message.versionCode = new CS("NE2010NO");
            message.receiver = new[] {
                new MCCI_MT000100UV01Receiver {
                    typeCode = CommunicationFunctionType.RCV,
                    device = new MCCI_MT000100UV01Device {
                        classCode = EntityClassDevice.DEV,
                        determinerCode = EntityDeterminerSpecific.INSTANCE,
                        id = new[] {
                            new II("2.16.578.1.12.4.5.1.1", null),
                        }
                    }
                }
            };
            return message;
        }

        private static PersonalInformation ReadPersonalInformation(bool askForIdNumber, bool askForGender)
        {
            var info = new PersonalInformation();
            if (askForIdNumber)
                info.FhNumber = ReadLineAndTrim("FH-number: ");
            info.FirstName = ReadLineAndTrim("First name(s): ");
            info.MiddleName = ReadLineAndTrim("Middle name(s): ");
            info.LastName = ReadLineAndTrim("Last name(s): ");
            info.DateOfBirth = ReadLineAndTrim(string.Format("Date of birth ({0}): ", DateFormat));
            info.StreetAddressLine = ReadLineAndTrim("Street address line: ");
            info.ZipCode = ReadLineAndTrim("Zip code: ");
            if (askForGender)
                info.Gender = ReadLineAndTrim("Gender (M/F): ");
            return info;
        }

        private static string ReadLineAndTrim(string message)
        {
            Console.Write(message);
            return (Console.ReadLine() ?? "").Trim();
        }
    }
}
