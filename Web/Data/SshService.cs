using Microsoft.Build.Framework;
using Renci.SshNet;

namespace Web_for_IotProject.Data
{
    public class SshService
    {
        private readonly string host = "10.45.139.210"; // Raspberry Pi IP
        private readonly string username = "root";
        public readonly string password = "changeme"; // Store securely in real applications

        //    public SshCommandResult ExecuteCommand(string command)
        //    {
        //        try
        //        {
        //            using var client = new SshClient(host, username, password);
        //            client.Connect();



        //            var result = client.RunCommand(command);


        //            client.Disconnect();

        //            return new SshCommandResult
        //            {
        //                Output = result.Result,
        //                Error = result.Error
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            return new SshCommandResult
        //            {
        //                Output = "",
        //                Error = ex.Message
        //            };
        //        }
        //    }
        //}
        public SshCommandResult ExecuteCommand(string command, bool useRawCommand = false)
        {
            using var client = new SshClient(host, username, password);
            try
            {
                client.Connect();
                if (!client.IsConnected)
                    return new SshCommandResult("", "SSH connection failed", false);

                string finalCommand = useRawCommand ? command : $"echo '{password}' | sudo -S {command}";
                var sshCommand = client.CreateCommand(finalCommand);
                var result = sshCommand.Execute();

                return new SshCommandResult(result, sshCommand.Error, false);
            }
            catch (Exception ex)
            {
                return new SshCommandResult("", ex.Message,false);
            }
            finally
            {
                if (client.IsConnected)
                    client.Disconnect();
            }
        }

        public class SshCommandResult
        {
            public string Output { get; set; }
            public string Error { get; set; }
            public bool IsSuccess { get; set; }

            public SshCommandResult(string output, string error, bool isSuccess)
            {
                Output = output;
                Error = error;
                IsSuccess = isSuccess;
            }

            public bool Success => string.IsNullOrEmpty(Error);
        }
    }
}
