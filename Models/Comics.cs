using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GahuahwaMainService.Models;

public class Comics  {
    
    [Key] 
    public int ComicId { get; set; }
    
    [Column(TypeName ="nvarchar(100)")]
    public string ComicName { get; set; } = "";
    
    [Column(TypeName = "nvarchar(2083)")]
    public string ComicUrl { get; set; } = "";
    
    [Column(TypeName = "float")]
    public double LastReadChapter { get; set; }
    
    [Column(TypeName = "int")]
    public int rating { get; set; }

}