using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
            using (client)
            {
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
                            RecordRevised(client);
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
                        client.Abort();
                        ((IDisposable) client).Dispose();
                        client = CreateClient();
                    }
                }
            }
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
            const string dateFormat = "yyyyMMdd";

            Console.Write("First name(s), or blank to skip: ");
            string firstName = (Console.ReadLine() ?? "").Trim();
            Console.Write("Last name(s), or blank to skip: ");
            string lastName = (Console.ReadLine() ?? "").Trim();
            Console.Write("Date of birth ({0}), or blank to skip: ", dateFormat);
            string dateOfBirth = (Console.ReadLine() ?? "").Trim();

            var paramList = new PRPA_MT101306NO01ParameterList();

            if (lastName != "" || firstName != "")
            {
                var nameItems = new List<ENXP>();
                nameItems.AddRange(firstName.Split().Select(fn => new engiven(fn)));
                nameItems.AddRange(lastName.Split().Select(ln => new enfamily(ln)));
                paramList.personName = CreatePersonNameParameter(nameItems);
            }

            if (dateOfBirth != "")
            {
                DateTime dummy;
                if (DateTime.TryParseExact(dateOfBirth, dateFormat, null, DateTimeStyles.None, out dummy))
                    paramList.personBirthTime = CreatePersonBirthTimeParameter(dateOfBirth);
                else
                    Console.WriteLine("Warning: Date of birth is illegal; skipping");
            }

            paramList.personAdministrativeGender = new[] {
                new PRPA_MT101306NO01PersonAdministrativeGender {value = new[] {new CE(), new CE()}},
                new PRPA_MT101306NO01PersonAdministrativeGender {value = new[] {new CE(), new CE()}},
            };

            var message = SetTopLevelFields(new PRPA_IN101305NO01 {
                controlActProcess = new PRPA_IN101305NO01QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101306NO01QueryByParameter {
                        parameterList = paramList
                    }
                }
            });

            FindCandidatesRequestSerializer.Serialize(Console.Out, message);
            Console.WriteLine();
            PRPA_IN101306NO01 result = client.FindCandidates(message);
            FindCandidatesResponseSerializer.Serialize(Console.Out, result);
            Console.WriteLine();

            Console.WriteLine("Found {0} persons:", result.controlActProcess.queryAck.resultTotalQuantity.value);
            if (result.controlActProcess.subject != null)
                foreach (var subject in result.controlActProcess.subject)
                    Console.WriteLine(PersonToString(subject.registrationEvent.subject1.identifiedPerson));
        }

        private static void GetDemographics(PersonRegistryClient client)
        {
            Console.Write("Enter id number: ");
            string idNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(idNumber))
                return;

            var id = new II {root = GetOid(idNumber.Trim()), extension = idNumber.Trim()};
            PRPA_IN101307NO01 request = CreateGetDemographicsRequest(id);

            GetDemographicsRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine();
            PRPA_IN101308NO01 response = client.GetDemographics(request);
            GetDemographicsResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine();

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
            var request = SetTopLevelFields(new PRPA_IN101311NO01 {
                controlActProcess = new PRPA_IN101311NO01MFMI_MT700721UV01ControlActProcess {
                    subject = new PRPA_IN101311NO01MFMI_MT700721UV01Subject1 {
                        registrationRequest = new PRPA_IN101311NO01MFMI_MT700721UV01RegistrationRequest {
                            subject1 = new PRPA_IN101311NO01MFMI_MT700721UV01Subject2 {
                                identifiedPerson = new PRPA_MT101311NO01IdentifiedPerson {
                                    identifiedPerson = new PRPA_MT101311NO01Person {
                                        birthTime = new TS {value = "19850924"}
                                    }
                                }
                            }
                        }
                    }
                }
            });
            
            AddPersonRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine();
            PRPA_IN101319NO01 response = client.AddPerson(request);
            AddPersonOrRevisePersonRecordResponseSerializer.Serialize(Console.Out, response);
            Console.WriteLine();
        }

        private static void RecordRevised(PersonRegistryClient client)
        {
            Console.Write("Enter id number: ");
            string idNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(idNumber))
                return;

            var request = SetTopLevelFields(new PRPA_IN101314NO01 {
                controlActProcess = new PRPA_IN101314NO01MFMI_MT700721UV01ControlActProcess {
                    subject = new PRPA_IN101314NO01MFMI_MT700721UV01Subject1 {
                        registrationRequest = new PRPA_IN101314NO01MFMI_MT700721UV01RegistrationRequest {
                            subject1 = new PRPA_IN101314NO01MFMI_MT700721UV01Subject2 {
                                identifiedPerson = new PRPA_MT101302NO01IdentifiedPerson {
                                    id = new[] {new II {root = GetOid(idNumber), extension = idNumber}},
                                    identifiedPerson = new PRPA_MT101302NO01Person {
                                        administrativeGenderCode = new CE {codeSystem = "2.16.840.1.113883.5.1", code = "M"},
                                        birthTime = new TS {value = "19480526"},
                                        name = new[] {
                                            new PN {
                                                use = new[] {EntityNameUse.OR},
                                                Items = new ENXP[] {
                                                    new engiven {Text = new[] {"Ole"}},
                                                    new engiven {Text = new[] {"Petter"}},
                                                    new enfamily {qualifier = new[] {EntityNamePartQualifier.MID}, Text = new[] {"Fjell"}},
                                                    new enfamily {qualifier = new[] {EntityNamePartQualifier.MID}, Text = new[] {"Berg"}},
                                                    new enfamily {Text = new[] {"Bang"}},
                                                    new enfamily {Text = new[] {"Hansen"}}
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            });

            RevisePersonRecordRequestSerializer.Serialize(Console.Out, request);
            Console.WriteLine();
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
            Console.WriteLine();
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
            Console.WriteLine();
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

        private static PRPA_MT101306NO01PersonName[] CreatePersonNameParameter(IEnumerable<ENXP> nameItems)
        {
            return new[] {
                new PRPA_MT101306NO01PersonName {
                    value = new[] {
                        new PN {
                            Items = nameItems.ToArray()
                        }
                    }
                }
            };
        }

        private static PRPA_MT101306NO01PersonBirthTime[] CreatePersonBirthTimeParameter(string dateOfBirth)
        {
            return new[] {
                new PRPA_MT101306NO01PersonBirthTime {
                    value = new[] {
                        new IVL_TS {value = dateOfBirth}
                    }
                }
            };
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
                sb.Append(string.Join(" ", person.addr[0].Items.Select(ai => ai.Text[0])));
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
    }
}
