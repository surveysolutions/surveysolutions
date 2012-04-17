using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class AgentsData : ModuleData {
        ObservableCollection<Agent> agents;

        public ObservableCollection<Agent> Agents {
            get { return agents; }
            set { SetValue<ObservableCollection<Agent>>("Agents", ref agents, value); }
        }
        public override void Load() {
            base.Load();
            Agents = new ObservableCollection<Agent>();
            foreach(Agent agent in DataSource.Current.GetAgents()) {
                Agents.Add(agent);
            }
        }
    }
    public class Agents : ModuleWithNavigator {
        static Agent savedCurrentAgent;
        Agent currentAgent;
        AgentDetail detail;

        public Agents() {
            Title = "Agents";
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            Agent neededAgent = parameter as Agent;
            if(neededAgent == null)
                neededAgent = savedCurrentAgent;
            CurrentAgent = neededAgent == null ? AgentsData.Agents[0] : neededAgent;
        }
        public override void SaveData() {
            base.SaveData();
            savedCurrentAgent = CurrentAgent;
        }
        public override List<Module> GetSubmodules() {
            List<Module> submodules = base.GetSubmodules();
            submodules.Add(Detail);
            return submodules;
        }
        public AgentsData AgentsData { get { return (AgentsData)Data; } }

        public Agent CurrentAgent {
            get { return currentAgent; }
            set { SetValue<Agent>("CurrentAgent", ref currentAgent, value, RaiseCurrentAgentChanged); }
        }

        public AgentDetail Detail {
            get { return detail; }
            private set { SetValue<AgentDetail>("Detail", ref detail, value); }
        }
        void RaiseCurrentAgentChanged(Agent oldValue, Agent newValue) {
            Detail = (AgentDetail)ModulesManager.CreateModule(Detail, new AgentDetailData(newValue), this);
        }
        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();
            SetCurrentAgentCommand = new SimpleActionCommand(DoSetCurrentAgent);
        }
        void DoSetCurrentAgent(object p) { CurrentAgent = p as Agent; }
        public ICommand SetCurrentAgentCommand { get; private set; }
        #endregion
    }
}
