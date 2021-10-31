using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPHandlerNameSpace;

namespace LudaviTest
{
    [TestClass]
    public class TCPHandlerTest
    {
        [TestMethod]
        public void SendMessage_ToServer_DataArray()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            string room = "1";
            string message = "Hallo dit is een test!";
            string fullMessageString = $"{ guid.ToString()} {room} {((int)TCPHandler.MessageTypes.CHAT).ToString() } {message}";
            string[] fullMessage = {guid.ToString(), room, ((int)TCPHandler.MessageTypes.CHAT).ToString(), message};
            byte[] dataStrinArray = Encoding.ASCII.GetBytes(fullMessageString);
            byte[] length = BitConverter.GetBytes(fullMessageString.Length);
            byte[] testMessage = new byte[dataStrinArray.Length + 4];
            length.CopyTo(testMessage, 0);
            dataStrinArray.CopyTo(testMessage, 4);

            Mock<INetworkStream> mockTCP = new Mock<INetworkStream>();
            TCPHandler handler = new TCPHandler(mockTCP.Object);

            //Act
            mockTCP.Setup(x => x.Write(It.IsAny<byte[]>())).Verifiable();
            handler.SendMessage(fullMessage);


            //Assert
            mockTCP.Verify(x => x.Write(testMessage), Times.Once);
        }

        [TestMethod]
        public void SendMessage_ToServer_Params()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            string room = "1";
            string message = "Hallo dit is een test!";
            string fullMessageString = $"{ guid.ToString()} {room} {((int)TCPHandler.MessageTypes.CHAT).ToString() } {message}";
            string[] fullMessage = { guid.ToString(), room, ((int)TCPHandler.MessageTypes.CHAT).ToString(), message };
            byte[] dataStrinArray = Encoding.ASCII.GetBytes(fullMessageString);
            byte[] length = BitConverter.GetBytes(fullMessageString.Length);
            byte[] testMessage = new byte[dataStrinArray.Length + 4];
            length.CopyTo(testMessage, 0);
            dataStrinArray.CopyTo(testMessage, 4);

            Mock<INetworkStream> mockTCP = new Mock<INetworkStream>();
            TCPHandler handler = new TCPHandler(mockTCP.Object);

            //Act
            mockTCP.Setup(x => x.Write(It.IsAny<byte[]>())).Verifiable();
            handler.SendMessage(guid, room, TCPHandler.MessageTypes.CHAT, message);


            //Assert
            mockTCP.Verify(x => x.Write(testMessage), Times.Once);
            
        }

        [TestMethod]
        public void ReadData_FromServer()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            string room = "1";
            string message = "Hallo dit is een test!";
            string fullMessageString = $"{ guid.ToString()} {room} {((int)TCPHandler.MessageTypes.CHAT).ToString() } {message}";
            string[] fullMessage = { guid.ToString(), room, ((int)TCPHandler.MessageTypes.CHAT).ToString(), message };
            byte[] dataStrinArray = Encoding.ASCII.GetBytes(fullMessageString);
            byte[] length = BitConverter.GetBytes(fullMessageString.Length);
            byte[] testMessage = new byte[dataStrinArray.Length + 4];
            length.CopyTo(testMessage, 0);
            dataStrinArray.CopyTo(testMessage, 4);

            Mock<INetworkStream> mockTCP = new Mock<INetworkStream>();
            TCPHandler handler = new TCPHandler(mockTCP.Object);

            //Act 
            byte[] tempTestMessage = new byte[testMessage.Length];
            testMessage.CopyTo(tempTestMessage, 0);
            mockTCP.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int size) => 
            { 
                Array.Copy(tempTestMessage, offset, buffer, 0, size);
                tempTestMessage = new byte[testMessage.Length - 4];
                Array.Copy(testMessage, 4, tempTestMessage, 0, tempTestMessage.Length);
                }).Returns((byte[] buffer, int offset, int size) => size);
            string[] messageStringArray = handler.ReadMessage();

            //Assert
            mockTCP.Verify(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
            Assert.AreEqual<string>(fullMessage[0], messageStringArray[0]);
            Assert.AreEqual<string>(fullMessage[1], messageStringArray[1]);
            Assert.AreEqual<string>(fullMessage[2], messageStringArray[2]);
            Assert.AreEqual<string>(fullMessage[3], messageStringArray[3]);
        }

    }
}
