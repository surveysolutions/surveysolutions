using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    // ReSharper disable InconsistentNaming

    public class InterviewExpressionMate : IInterviewExpressionState
    {
        public InterviewExpressionMate()
        {

            var RosterGroupInstance = new Dictionary<string, DistinctDecimalList>();

            //build by interview or interview state
            this.main = new Main_Intervew();
            
            
            
            var roster_Person = new Roster_Person(this.main, new decimal []{ 0 });
            var roster_Pet = new Roster_Pet(roster_Person, new decimal [] {0, 0});

            this.allRosters.Add(roster_Person);
            
            this.allRosters.Add(roster_Pet);

        }

        private Dictionary<string, IRoster> RosterInstances { set; get; }


        
        public void AddRosterInstances(IEnumerable<RosterInstanceItem> instances)
        {
            foreach (var rosterInstance in instances)
            {

                this.AddRoster(rosterInstance.GroupId, rosterInstance.OuterRosterVector, rosterInstance.RosterInstanceId);
                //add roster instance
            }
        }

        private void AddRoster(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            
        }

        public void RemoveRosterInstances(IEnumerable<RosterInstanceItem> instances)
        {
            foreach (var rosterInstance in instances)
            {
                //remove roster instance
            }
        }

        public void SetAnswer(Guid QuestionId, decimal[] PropagationVector)
        {
            //set value
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

            validation.AddRange(this.main.PerformValidation());

            foreach (var validationPerformer in this.allRosters)
            {
                validation.AddRange(validationPerformer.PerformValidation());
            }

            return validation;
        }
        
        public class Main_Intervew : IValidationPerformer, IRoster
        {
            public Guid id { get; set; }
            public string title { get; set; }
            public int rooms { get; set; }

            public decimal[] RosterVector { get; private set; }

            public Dictionary<Guid, List<object>> rosters = new Dictionary<Guid, List<object>>();


            public Roster_Person[] persons = new Roster_Person[0];

            private Dictionary<Guid, Func<bool>> validations;

            public List<Identity> PerformValidation()
            {
                List<Identity> invalidIdentities = new List<Identity>();

                foreach (var validation in this.validations)
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
                
                this.validations.Add(name_questionId, this.name_validate);
                
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
                return this.name.Length > 0;
            }

            public bool name_evaluate()
            {
                return this.rooms > 0;
            }

            public List<Identity> PerformValidation()
            {
                List<Identity> invalidIdentities = new List<Identity>();

                foreach (var validation in this.validations)
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

                foreach (var validation in this.validations)
                {
                    if (!validation.Value())
                        invalidIdentities.Add(new Identity(validation.Key, this.RosterVector));
                }

                return invalidIdentities;
            }
        }

        public void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, long answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            throw new NotImplementedException();
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
            throw new NotImplementedException();
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector)
        {
            throw new NotImplementedException();
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors)
        {
            throw new NotImplementedException();
        }

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            throw new NotImplementedException();
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            throw new NotImplementedException();
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            throw new NotImplementedException();
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            throw new NotImplementedException();
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            throw new NotImplementedException();
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            throw new NotImplementedException();
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
            throw new NotImplementedException();
        }

        public void ProcessValidationExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            throw new NotImplementedException();
        }

        public void ProcessConditionExpressions(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
        }

        public IInterviewExpressionState Clone()
        {
            throw new NotImplementedException();
        }

        internal static string ConvertIdAndRosterVectorToString(Guid id, decimal[] rosterVector)
        {
            return string.Format("{0:N}[{1}]", id, string.Join("-", rosterVector.Select(v => v.ToString("0.############################", CultureInfo.InvariantCulture))));
        }
    }

    public interface IRoster {}


    public interface IValidationPerformer
    {
        List<Identity> PerformValidation();
    }



    public class RosterInstanceItem
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }

        public RosterInstanceItem(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
        }
    }



    internal class DistinctDecimalList : IEnumerable<decimal>
    {
        private readonly List<decimal> list = new List<decimal>();

        public DistinctDecimalList() { }

        public DistinctDecimalList(IEnumerable<decimal> decimals)
            : this()
        {
            this.list.AddRange(decimals.Distinct());
        }

        public void Add(decimal value)
        {
            if (!this.list.Contains(value))
            {
                this.list.Add(value);
            }
        }

        public void Remove(decimal value)
        {
            this.list.Remove(value);
        }

        public bool Contains(decimal value)
        {
            return this.list.Contains(value);
        }

        public IEnumerator<decimal> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.list).GetEnumerator();
        }
    }



    // ReSharper restore InconsistentNaming
}
