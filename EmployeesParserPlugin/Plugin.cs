using EmployeesParserPlugin.APIs.DummyJson;
using Newtonsoft.Json;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace EmployeesParserPlugin
{
    [Author(Name = "Dmitry")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private const string apiUri = "https://dummyjson.com/users";

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Starting Parser");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var employeesList = args.Cast<EmployeesDTO>()?.ToList() ?? new List<EmployeesDTO>();

            using (HttpClient client = new HttpClient())
            {
                ApiResponse api = null;

                try
                {
                    var response = client.GetAsync(apiUri).Result;
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        api = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
                    }
                    catch (JsonException ex)
                    {
                        logger.Error($"JSON deserialization error: {ex.Message}");
                        logger.Trace(ex.StackTrace);
                        return args;
                    }
                }
                catch (HttpRequestException ex)
                {
                    logger.Error($"Something went wrong with request: {ex.Message}");
                    logger.Trace(ex.StackTrace);
                    return args;
                }
                catch (AggregateException ex)
                {
                    logger.Error($"Error occured: {ex.Message}");
                    logger.Trace(ex.StackTrace);
                    return args;
                }

                // Read until number is valid.
                //
                int countToParse;
                int maxToParse = api.Limit;
                while (true)
                {
                    Console.Write($"Count of employees to parse (0 to {maxToParse}): ");
                    string inputCount = Console.ReadLine();

                    bool isValidNumber = Int32.TryParse(inputCount, out countToParse);
                    if (! isValidNumber)
                    {
                        logger.Warn("Not an int value! Please enter a valid integer.");
                        continue;
                    }
                    if (countToParse < 0 || countToParse > maxToParse)
                    {
                        logger.Warn($"The value must be between 0 and {maxToParse}.");
                        continue;
                    }

                    logger.Info($"Parsing {countToParse} employees...");
                    break;
                }

                // Parse {countToParse} users.
                //
                int countParsed = 0;
                for (int i = 0; i < countToParse; i++, countParsed++)
                {
                    var user = api.Users[i];

                    var employee = new EmployeesDTO()
                    {
                        Name = $"{user.FirstName} {user.LastName}"
                    };
                    employee.AddPhone(user.Phone);

                    employeesList.Add(employee);
                }
                logger.Info($"Added {countParsed} employees.");
            }

            return employeesList.Cast<DataTransferObject>();
        }
    }
}
