namespace Bless_Screen_Fix.Models
{
    public class Port
    {
        public string Port_number { get; set; }
        public string Process_name { get; set; }
        public string Protocol { get; set; }
        public string Connection { get; set; }
        public override string ToString()
        {
            return $"{Process_name} ({Protocol} port {Port_number})";
        }
    }
}
