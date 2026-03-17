namespace SmartRollCall.Api.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FaceVectorJson { get; set; } = string.Empty; // Guarda el personId GUID de Azure (o embeddings futuros)
    }
}