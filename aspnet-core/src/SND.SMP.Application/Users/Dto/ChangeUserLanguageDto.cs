using System.ComponentModel.DataAnnotations;

namespace SND.SMP.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}