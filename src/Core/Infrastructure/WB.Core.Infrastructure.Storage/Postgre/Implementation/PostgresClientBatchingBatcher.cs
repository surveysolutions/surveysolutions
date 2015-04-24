using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text;
using NHibernate;
using NHibernate.AdoNet;
using Npgsql;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PostgresClientBatchingBatcher : AbstractBatcher
    {

        private int batchSize;
        private int countOfCommands = 0;
        private int totalExpectedRowsAffected;
        private StringBuilder sbBatchCommand;
        private int m_ParameterCounter;

        private IDbCommand currentBatch;

        public PostgresClientBatchingBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
            : base(connectionManager, interceptor)
        {
            this.batchSize = this.Factory.Settings.AdoBatchSize;
        }


        private string NextParam()
        {
            return ":p" + this.m_ParameterCounter++;
        }

        public override void AddToBatch(IExpectation expectation)
        {
            if (expectation.CanBeBatched && !(this.CurrentCommand.CommandText.StartsWith("INSERT INTO") && this.CurrentCommand.CommandText.Contains("VALUES")))
            {
                //NonBatching behavior
                IDbCommand cmd = this.CurrentCommand;
                this.LogCommand(this.CurrentCommand);
                int rowCount = this.ExecuteNonQuery(cmd);
                expectation.VerifyOutcomeNonBatched(rowCount, cmd);
                this.currentBatch = null;
                return;
            }

            this.totalExpectedRowsAffected += expectation.ExpectedRowCount;

            int len = this.CurrentCommand.CommandText.Length;
            int idx = this.CurrentCommand.CommandText.IndexOf("VALUES");
            int endidx = idx + "VALUES".Length + 2;

            if (this.currentBatch == null)
            {
                // begin new batch. 
                this.currentBatch = new NpgsqlCommand();
                this.sbBatchCommand = new StringBuilder();
                this.m_ParameterCounter = 0;

                string preCommand = this.CurrentCommand.CommandText.Substring(0, endidx);
                this.sbBatchCommand.Append(preCommand);
            }
            else
            {
                //only append Values
                this.sbBatchCommand.Append(", (");
            }

            //append values from CurrentCommand to sbBatchCommand
            string values = this.CurrentCommand.CommandText.Substring(endidx, len - endidx - 1);
            //get all values
            string[] split = values.Split(',');

            ArrayList paramName = new ArrayList(split.Length);
            for (int i = 0; i < split.Length; i++)
            {
                if (i != 0)
                    this.sbBatchCommand.Append(", ");

                string param = null;
                if (split[i].StartsWith(":"))   //first named parameter
                {
                    param = this.NextParam();
                    paramName.Add(param);
                }
                else if (split[i].StartsWith(" :")) //other named parameter
                {
                    param = this.NextParam();
                    paramName.Add(param);
                }
                else if (split[i].StartsWith(" "))  //other fix parameter
                {
                    param = split[i].Substring(1, split[i].Length - 1);
                }
                else
                {
                    param = split[i];   //first fix parameter
                }

                this.sbBatchCommand.Append(param);
            }
            this.sbBatchCommand.Append(")");

            //rename & copy parameters from CurrentCommand to currentBatch
            int iParam = 0;
            foreach (NpgsqlParameter param in this.CurrentCommand.Parameters)
            {
                param.ParameterName = (string)paramName[iParam++];

                NpgsqlParameter newParam = /*Clone()*/new NpgsqlParameter(param.ParameterName, param.NpgsqlDbType, param.Size, param.SourceColumn, param.Direction, param.IsNullable, param.Precision, param.Scale, param.SourceVersion, param.Value);
                this.currentBatch.Parameters.Add(newParam);
            }

            this.countOfCommands++;
            //check for flush
            if (this.countOfCommands >= this.batchSize)
            {
                this.DoExecuteBatch(this.currentBatch);
            }
        }

        protected override void DoExecuteBatch(IDbCommand ps)
        {
            if (this.currentBatch != null)
            {
                //Batch command now needs its terminator
                this.sbBatchCommand.Append(";");

                this.countOfCommands = 0;

                this.CheckReaders();

                //set prepared batchCommandText
                string commandText = this.sbBatchCommand.ToString();
                this.currentBatch.CommandText = commandText;

                this.LogCommand(this.currentBatch);

                this.Prepare(this.currentBatch);

                int rowsAffected = this.currentBatch.ExecuteNonQuery();

                Expectations.VerifyOutcomeBatched(this.totalExpectedRowsAffected, rowsAffected);

                this.totalExpectedRowsAffected = 0;
                this.currentBatch = null;
                this.sbBatchCommand = null;
                this.m_ParameterCounter = 0;
            }
        }

        protected override int CountOfStatementsInCurrentBatch
        {
            get { return this.countOfCommands; }
        }

        public override int BatchSize
        {
            get { return this.batchSize; }
            set { this.batchSize = value; }
        }
    }
}