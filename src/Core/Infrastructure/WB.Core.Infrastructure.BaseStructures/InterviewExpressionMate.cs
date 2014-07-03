using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.BaseStructures
{
    // ReSharper disable InconsistentNaming

    public class InterviewExpressionMate : IInterviewEvaluator
    {
        public InterviewExpressionMate()
        {
            //build by interview or interview state
            main = new Main_Intervew();

            var roster_Person = new Roster_Person(main, new decimal []{ 0 });

            var roster_Pet = new Roster_Pet(roster_Person, new decimal [] {0, 0});

            allRosters.Add(roster_Person);

            allRosters.Add(roster_Pet);

        }

        private Main_Intervew main;
        private List<IValidationPerformer> allRosters; 


        public int Test()
        {
            return 0;
        }

        
        public List<Identity> CalculateConditionChanges()
        {
            throw new NotImplementedException();
        }

        public List<Identity> CalculateValidationChanges()
        {
            var validation = new List<Identity>();

            validation.AddRange(main.PerformValidation());

            foreach (var validationPerformer in allRosters)
            {
                validation.AddRange(validationPerformer.PerformValidation());
            }

            return validation;
        }
        
        public class Main_Intervew : IValidationPerformer
        {
            public Guid id { get; set; }
            public string title { get; set; }
            public int rooms { get; set; }

            public decimal[] RosterVector { get; private set; }


            public Roster_Person[] persons;


            private Dictionary<Guid, Func<bool>> validations;

            public List<Identity> PerformValidation()
            {
                List<Identity> invalidIdentities = new List<Identity>();

                foreach (var validation in validations)
                {
                    if (!validation.Value())
                        invalidIdentities.Add(new Identity(validation.Key, this.RosterVector));
                }

                return invalidIdentities;
            }
        }

        public class Roster_Person : IValidationPerformer
        {
            public Roster_Person(Main_Intervew parent, decimal[] rosterVector)
            {
                this.parent = parent;
                this.RosterVector = rosterVector;

                //set from template
                Guid name_questionId = Guid.Parse("111111111111111111111111");
                
                validations.Add(name_questionId, name_validate);
                
            }

            private Dictionary<Guid, Func<bool>> validations;

            public decimal[] RosterVector { get; private set; }

            private Main_Intervew parent;

            public Roster_Pet[] pets;
            
            public Guid id { get { return this.parent.id; } }
            public string title { get { return this.parent.title; } }
            public int rooms { get { return this.parent.rooms; }}

            public string name { set; get; }
            

            //generated
            public bool name_validate()
            {
                return name.Length > 0;
            }

            public bool name_evaluate()
            {
                return rooms > 0;
            }

            public List<Identity> PerformValidation()
            {
                List<Identity> invalidIdentities = new List<Identity>();

                foreach (var validation in validations)
                {
                    if(!validation.Value())
                        invalidIdentities.Add(new Identity(validation.Key, this.RosterVector));
                }

                return invalidIdentities;
            }

            public Roster_Person GetCopy(Main_Intervew parent) 
            {
                return new Roster_Person(parent, this.RosterVector);
            }

        }

        public class Roster_Pet : IValidationPerformer
        {
            public Roster_Pet(Roster_Person parent, decimal[] rosterVector)
            {
                this.parent = parent;
                this.RosterVector = rosterVector;
            }

            private Dictionary<Guid, Func<bool>> validations;
            
            public decimal[] RosterVector { get; set; }


            private Roster_Person parent;


            public Guid id { get { return this.parent.id; } }
            public string title { get { return this.parent.title; } }
            public int rooms { get { return this.parent.rooms; } }

            public string name { get { return this.parent.name; } }

            public string pet_name { get; set; }


            public List<Identity> PerformValidation()
            {
                List<Identity> invalidIdentities = new List<Identity>();

                foreach (var validation in validations)
                {
                    if (!validation.Value())
                        invalidIdentities.Add(new Identity(validation.Key, this.RosterVector));
                }

                return invalidIdentities;
            }
        }
    }

    
    public interface IValidationPerformer
    {
        List<Identity> PerformValidation();
    }

    
    // ReSharper restore InconsistentNaming
}
