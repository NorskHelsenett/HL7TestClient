using System;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using HL7TestClient.PersonRegistry;

namespace HL7TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var client = new PersonRegistryClient())
                {
                    client.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["username"];
                    client.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["password"];

                    while (true)
                    {
                        Console.Write("Would you like to (F)indCandidates, (G)etDemographics, or (E)xit? ");
                        string action = Console.ReadLine().Trim().ToUpper();
                        if (action == "F")
                            FindCandidates(client);
                        else if (action == "G")
                            GetDemographics(client);
                        else if (action == "E")
                            break;
                    }
                }
            }
            catch (FaultException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                do
                {
                    Console.WriteLine(e);
                    e = e.InnerException;
                } while (e != null);
            }
            Console.ReadLine();
        }

        public static void FindCandidates(PersonRegistryClient client)
        {
            Console.Write("Last name: ");
            string lastName = Console.ReadLine().Trim();

            //PRPA_MT101306UV02PersonBirthTime birthTime = new PRPA_MT101306UV02PersonBirthTime { value = new[] { new IVL_TS { value = "19850101" } } };
            var name = new PRPA_MT101306UV02PersonName
                           {value = new[] {new PN {Items = new ENXP[] {/*new engiven {Text = new[] {firstName}},*/ new enfamily {Text = new[] {lastName}}}}}};
            var paramList = new PRPA_MT101306UV02ParameterList {/*personBirthTime = new[] {birthTime}, */personName = new[] {name}};
            var query = new PRPA_MT101306UV02QueryByParameter {parameterList = paramList};
            var cact = new PRPA_IN101305UV02QUQI_MT021001UV01ControlActProcess {queryByParameter = query};
            var message = new PRPA_IN101305NO {controlActProcess = cact, processingCode = new CS {code = "T"}};

            var result = client.FindCandidates(message);
            Console.WriteLine(result.controlActProcess.queryAck.resultTotalQuantity.value);
            if (result.controlActProcess.subject != null)
                foreach (var subject in result.controlActProcess.subject)
                    Console.WriteLine(subject.registrationEvent.subject1.identifiedPerson.id[0].extension);
        }

        public static void GetDemographics(PersonRegistryClient client)
        {
            Console.Write("Enter id number: ");
            string idNumber = Console.ReadLine().Trim();

            var id = new II {root = "2.16.578.1.12.4.1.4.1", extension = idNumber};
            var getDemMessage = new PRPA_IN101307NO {
                processingCode = new CS {code = "T"},
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
            var getDemographicsResult = client.GetDemographics(getDemMessage);
            var nameElement = getDemographicsResult.controlActProcess.subject[0].registrationEvent.subject1.identifiedPerson.identifiedPerson.name[0];
            Console.WriteLine(string.Join(" ", nameElement.Items.Select(ni => ni.Text[0])));
        }
    }
}
