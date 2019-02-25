using System.Text;
using Npgsql;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class ExportBulkCommand
    {
        private readonly NpgsqlCommand command = new NpgsqlCommand();

        public NpgsqlParameterCollection Parameters => command.Parameters;

        public StringBuilder CommandText { get; } = new StringBuilder();

        public NpgsqlCommand GetCommand()
        {
            command.CommandText = CommandText.ToString();
            return command;
        }
    }
}
