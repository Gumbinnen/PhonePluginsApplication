using System.Collections.Generic;

namespace EmployeesParserPlugin.APIs.DummyJson
{
    public class ApiResponse
    {
        public List<ApiUser> Users { get; set; }
        public int Total {  get; set; }
        public int Limit { get; set; }
    }
}
