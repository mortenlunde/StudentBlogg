using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentBlogg.Feature;

[Table("Logs")]
public class Log
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public DateTime TimeStamp { get; set; }
    
    [MaxLength(128)]
    public string? Level { get; set; }
    
    [Column(TypeName = "Template")]
    public string? MessageTemplate { get; set; }
    
    public string? Message { get; set; }
    
    public string? Exception { get; set; }
    
    public string? Properties { get; set; }
}