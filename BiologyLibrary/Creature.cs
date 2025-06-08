using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiologyLibrary
{
        public class Creature
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool CanFly { get; set; } // свойство для статистики

            public override string ToString()
            {
                return $"Id: {Id}, Name: {Name}, CanFly: {CanFly}";
            }
        }
    }