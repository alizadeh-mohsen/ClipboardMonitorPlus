using Microsoft.EntityFrameworkCore;

namespace VisaChecker
{
    [Index(nameof(Name))]
    public class Gov
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Type { get; set; }
        public string Route { get; set; }
    }
}
