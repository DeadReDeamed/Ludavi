using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TCPHandlerNameSpace.Models;

namespace Server
{
    class FileIO
    {


        public static void WriteUsersToSaveFile(Dictionary<Guid, ServerClient> dict) {

            var fileFolder = Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "Ludavi");

            if (!Directory.Exists(fileFolder)) Directory.CreateDirectory(fileFolder);

            var fileName = Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "Ludavi/clients.dat");

            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
            };

            string jsonString = JsonSerializer.Serialize(dict, options);

            File.WriteAllText(fileName, jsonString);
        }

        public static void TryGetUsersFromSaveFile(ref Dictionary<Guid, ServerClient> clients)
        {
            IFormatter formatter = new BinaryFormatter();

            var fileName = Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "Ludavi/clients.dat");

            if (!File.Exists(fileName)) return;

            JsonElement clientsJson = JsonDocument.Parse(File.ReadAllText(fileName)).RootElement;

            foreach(JsonProperty user in clientsJson.EnumerateObject())
            {
                JsonElement UserObj = user.Value.GetProperty("User");
                Guid userId = Guid.Parse( UserObj.GetProperty("UserId").GetString() );
                string userName = UserObj.GetProperty("Name").GetString();

                clients.Add(userId, new ServerClient(new User(userName, userId)));   
            }

        }

    }
}
