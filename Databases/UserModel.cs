using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Databases
{
    [Table("users")]
    public class UserModel
    {
        [Column("id"), Key]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("username")]
        public string UserName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("money")]
        public int Money { get; set; }

        [NotMapped]
        public bool IsOnline { get; set; } = false;
    }
}
