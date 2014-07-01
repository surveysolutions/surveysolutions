using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming


namespace WB.Core.Infrastructure.BaseStructures
{
    public class Identity
    {
        // should be shared
        public Guid Id { get; private set; }
        public decimal[] RosterVector { get; private set; }

        public Identity(Guid id, decimal[] rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }
    }

    public interface IValidatable
    {
        void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }
    public class MainLevel : IValidatable
    {
        private readonly decimal[] rosterVector;

        public MainLevel(decimal[] rosterVector)
        {
            this.rosterVector = rosterVector;
        }

        public string id;
        public int? persons_count;

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid) { }
    }

    public class HhMember : IValidatable
    {
        private readonly decimal[] rosterVector;

        public HhMember(decimal[] rosterVector)
        {
            this.rosterVector = rosterVector;
        }

        private MainLevel parent;

        public string id { get { return parent.id; } }
        public int? persons_count { get { return parent.persons_count; } }

        public string name { get; set; }
        public int? age { get; set; }
        public DateTime? date { get; set; }
        public decimal? sex { get; set; }
        public decimal? role { get; set; }
        public decimal?[] food { get; set; }
        public decimal? has_job { get; set; }
        public string job_title { get; set; }
        public decimal? best_job_owner { get; set; }

        private bool age_Validation()
        {
            return age >= 3;
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            //if (age_Validation().CanBeProcessed)
            {
                var ageId = new Identity(Guid.Parse("00000000000000000000000000000001"), rosterVector);
                if (age_Validation())
                    questionsToBeValid.Add(ageId);
                else
                    questionsToBeInvalid.Add(ageId);
            }
        }
    }

    public class FoodConsumption : IValidatable
    {
        private readonly decimal[] rosterVector;
        public FoodConsumption(decimal[] rosterVector)
        {
            this.rosterVector = rosterVector;
        }

        private HhMember parent;

        public string id { get { return parent.id; } }
        public int? persons_count { get { return parent.persons_count; } }
        public string name { get { return parent.name; } }
        public int? age { get { return parent.age; } }
        public DateTime? date { get { return parent.date; } }
        public decimal? sex { get { return parent.sex; } }
        public decimal? role { get { return parent.role; } }
        public decimal?[] food { get { return parent.food; } }
        public decimal? has_job { get { return parent.has_job; } }
        public string job_title { get { return parent.job_title; } }
        public decimal? best_job_owner { get { return parent.best_job_owner; } }

        public int times_per_week;
        public decimal price_for_food;

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {

        }
    }

    public class StronglyTypedInterviewEvaluator : IInterviewEvaluator
    {
        private readonly Dictionary<decimal[], IValidatable> interviewScopes = new Dictionary<decimal[], IValidatable>(); 

        public int Test()
        {
            var questionsToBeValid = new List<Identity>();
            var questionsToBeInvalid = new List<Identity>();

            foreach (var interviewScope in interviewScopes.Values)
            {
                interviewScope.Validate(questionsToBeValid, questionsToBeInvalid);
            }
            return 8;
        }
    }
}
// ReSharper restore InconsistentNaming

