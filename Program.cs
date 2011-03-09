﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using HL7TestClient.Constants;
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

            var paramList = new PRPA_MT101306UV02ParameterList(); // {/*personBirthTime = new[] {birthTime}, */personName = new[] {name}};

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

            var message = new PRPA_IN101305NO {
                processingCode = ProcessingCode.Test(),
                controlActProcess = new PRPA_IN101305UV02QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101306UV02QueryByParameter {
                        parameterList = paramList
                    }
                }
            };

            PRPA_IN101306NO result = client.FindCandidates(message);

            Console.WriteLine("Found {0} persons:", result.controlActProcess.queryAck.resultTotalQuantity.value);
            if (result.controlActProcess.subject != null)
                foreach (var subject in result.controlActProcess.subject)
                    Console.WriteLine(subject.registrationEvent.subject1.identifiedPerson.id[0].extension);
        }

        private static void GetDemographics(PersonRegistryClient client)
        {
            Console.Write("Enter id number: ");
            string idNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(idNumber))
                return;

            var id = new II {root = IdNumberOid.FNumber, extension = idNumber.Trim()};
            var getDemMessage = new PRPA_IN101307NO {
                processingCode = ProcessingCode.Test(),
                controlActProcess = new PRPA_IN101307UV02QUQI_MT021001UV01ControlActProcess {
                    queryByParameter = new PRPA_MT101307UV02QueryByParameter {
                        parameterList = new PRPA_MT101307UV02ParameterList {
                            identifiedPersonIdentifier = new[] {
                                new PRPA_MT101307UV02IdentifiedPersonIdentifier {value = new[] {id}}
                            }
                        }
                    }
                }
            };

            PRPA_IN101308NO getDemographicsResult = client.GetDemographics(getDemMessage);

            string queryResponseCode = getDemographicsResult.controlActProcess.queryAck.queryResponseCode.code;
            switch (queryResponseCode)
            {
                case QueryResponseCode.Ok:
                    PN nameElement = getDemographicsResult.controlActProcess.subject[0].registrationEvent.subject1.identifiedPerson.identifiedPerson.name[0];
                    Console.WriteLine(string.Join(" ", nameElement.Items.Select(ni => ni.Text[0])));
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

        private static PRPA_MT101306UV02PersonName[] CreatePersonNameParameter(IEnumerable<ENXP> nameItems)
        {
            return new[] {
                new PRPA_MT101306UV02PersonName {
                    value = new[] {
                        new PN {
                            Items = nameItems.ToArray()
                        }
                    }
                }
            };
        }

        private static PRPA_MT101306UV02PersonBirthTime[] CreatePersonBirthTimeParameter(string dateOfBirth)
        {
            return new[] {
                new PRPA_MT101306UV02PersonBirthTime {
                    value = new[] {
                        new IVL_TS {value = dateOfBirth}
                    }
                }
            };
        }
    }
}
