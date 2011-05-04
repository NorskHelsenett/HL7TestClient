using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
        static void Main()
        {
            var client = CreateClient();
            using (client)
            {
                while (true)
                {
                    try
                    {
                        Console.Write("\nWould you like to (F)indCandidates, (G)etDemographics, or (E)xit? ");
                        string action = (Console.ReadLine() ?? "E").Trim().ToUpper();
                        if (action == "F")
                            FindCandidates(client);
                        else if (action == "G")
                            GetDemographics(client);
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

            var message = new PRPA_IN101305NO01 {
                processingCode = ProcessingCode.Test(),
                controlActProcess = new PRPA_IN101305NO01QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101306NO01QueryByParameter {
                        parameterList = paramList
                    }
                }
            };

            new XmlSerializer(typeof (PRPA_IN101305NO01), new XmlRootAttribute {Namespace = "urn:hl7-org:v3"}).Serialize(Console.Out, message);
            PRPA_IN101306NO01 result = client.FindCandidates(message);
            new XmlSerializer(typeof (PRPA_IN101306NO01), new XmlRootAttribute {Namespace = "urn:hl7-org:v3"}).Serialize(Console.Out, result);

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

            var id = new II {root = IdNumberOid.FNumber, extension = idNumber.Trim()};
            var getDemMessage = new PRPA_IN101307NO01 {
                processingCode = ProcessingCode.Test(),
                controlActProcess = new PRPA_IN101307NO01QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101307UV02QueryByParameter {
                        parameterList = new PRPA_MT101307UV02ParameterList {
                            identifiedPersonIdentifier = new[] {
                                new PRPA_MT101307UV02IdentifiedPersonIdentifier {value = new[] {id}}
                            }
                        }
                    }
                }
            };

            new XmlSerializer(typeof (PRPA_IN101307NO01), new XmlRootAttribute {Namespace = "urn:hl7-org:v3"}).Serialize(Console.Out, getDemMessage);
            PRPA_IN101308NO01 getDemographicsResult = client.GetDemographics(getDemMessage);
            new XmlSerializer(typeof (PRPA_IN101308NO01), new XmlRootAttribute {Namespace = "urn:hl7-org:v3"}).Serialize(Console.Out, getDemographicsResult);

            string queryResponseCode = getDemographicsResult.controlActProcess.queryAck.queryResponseCode.code;
            switch (queryResponseCode)
            {
                case QueryResponseCode.Ok:
                    Console.WriteLine(PersonToString(getDemographicsResult.controlActProcess.subject[0].registrationEvent.subject1.identifiedPerson));
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

        private static string PersonToString(IIdentifiedPerson identifiedPerson)
        {
            var sb = new StringBuilder();
            sb.Append(identifiedPerson.id[0].extension);
            sb.Append(": ");

            IPerson person = identifiedPerson.identifiedPerson;

            sb.Append(string.Join(" ", person.name[0].Items.Select(ni => ni.Text[0])));

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
    }
}
