using System;
using static TdkDataService.DictionaryServiceEnumerations;

namespace TdkDataService.Model.Entity
{
    public class Person
    {
        public Nullable<int> Id { get; set; }

        public String Root { get; set; }

        public String Name { get; set; }

        public GenderType Gender { get; set; }

        public String Meaning { get; set; }
    }
}
