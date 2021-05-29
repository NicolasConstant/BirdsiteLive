using System;
using System.IO;
using System.Runtime.CompilerServices;
using BirdsiteLive.Common.Settings;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.IsisMtt.X509;

namespace BSLManager.Tools
{
    public class SettingsManager
    {
        private const string LocalFileName = "ManagerSettings.json";

        public (DbSettings dbSettings, InstanceSettings instanceSettings) GetSettings()
        {
            var localSettingsData = GetLocalSettingsFile();
            if (localSettingsData != null) return Convert(localSettingsData);

            Console.WriteLine("We need to set up the manager");
            Console.WriteLine("Please provide the following information as provided in the docker-compose file");

            LocalSettingsData data;
            do
            {
                data = GetDataFromUser();
                Console.WriteLine();
                Console.WriteLine("Please check if all is ok:");
                Console.WriteLine();
                Console.WriteLine($"Db Host: {data.DbHost}");
                Console.WriteLine($"Db Name: {data.DbName}");
                Console.WriteLine($"Db User: {data.DbUser}");
                Console.WriteLine($"Db Password: {data.DbPassword}");
                Console.WriteLine($"Instance Domain: {data.InstanceDomain}");
                Console.WriteLine();

                string resp;
                do
                {
                    Console.WriteLine("Is it valid? (yes, no)");
                    resp = Console.ReadLine()?.Trim().ToLowerInvariant();

                    if (resp == "n" || resp == "no") data = null;

                } while (resp != "y" && resp != "yes" && resp != "n" && resp != "no");

            } while (data == null);
            
            SaveLocalSettings(data);
            return Convert(data);
        }

        private LocalSettingsData GetDataFromUser()
        {
            var data = new LocalSettingsData();

            Console.WriteLine("Db Host:");
            data.DbHost = Console.ReadLine();

            Console.WriteLine("Db Name:");
            data.DbName = Console.ReadLine();

            Console.WriteLine("Db User:");
            data.DbUser = Console.ReadLine();

            Console.WriteLine("Db Password:");
            data.DbPassword = Console.ReadLine();

            Console.WriteLine("Instance Domain:");
            data.InstanceDomain = Console.ReadLine();

            return data;
        }

        private (DbSettings dbSettings, InstanceSettings instanceSettings) Convert(LocalSettingsData data)
        {
            var dbSettings = new DbSettings
            {
                Type = data.DbType,
                Host = data.DbHost,
                Name = data.DbName,
                User = data.DbUser,
                Password = data.DbPassword
            };
            var instancesSettings = new InstanceSettings
            {
                Domain = data.InstanceDomain
            };
            return (dbSettings, instancesSettings);
        }

        private LocalSettingsData GetLocalSettingsFile()
        {
            try
            {
                if (!File.Exists(LocalFileName)) return null;

                var jsonContent = File.ReadAllText(LocalFileName);
                var content = JsonConvert.DeserializeObject<LocalSettingsData>(jsonContent);
                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SaveLocalSettings(LocalSettingsData data)
        {
            var jsonContent = JsonConvert.SerializeObject(data);
            File.WriteAllText(LocalFileName, jsonContent);
        }
    }

    internal class LocalSettingsData
    {
        public string DbType { get; set; } = "postgres";
        public string DbHost { get; set; }
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }

        public string InstanceDomain { get; set; }
    }
}