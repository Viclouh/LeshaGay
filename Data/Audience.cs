namespace API.Models
{
    public class Audience
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int? AudienceTypeId { get;set; }
        public AudienceType? AudienceType { get;set; }
    }
}
