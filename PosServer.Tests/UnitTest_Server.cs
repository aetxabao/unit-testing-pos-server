using Xunit;

namespace PosServer.Tests
{
    public class UnitTest_Server
    {

        [Fact]
        public void Test_AddMessage()
        {
            Server.repo.Clear();
            Assert.True(Server.repo.Count == 0, "El repositorio está vacío.");
            Message m1 = new Message { From = "22", To = "11", Msg = "Adeu!", Stamp = "A.E." };
            Server.AddMessage(m1);
            Assert.True(Server.repo.Count == 1, $"Hay mensajes para un destinatario.");
            Message m2 = new Message { From = "33", To = "11", Msg = "Adios!", Stamp = "A.E." };
            Server.AddMessage(m2);
            Assert.True(Server.repo.Count == 1, $"Hay mensajes para un destinatario.");
            Message m3 = new Message { From = "22", To = "33", Msg = "Agur!", Stamp = "A.E." };
            Server.AddMessage(m3);
            Assert.True(Server.repo.Count == 2, $"Hay mensajes para dos destinatarios.");
            Assert.True(Server.repo["33"].Count == 1, $"El 33 tiene un mensaje.");
            Assert.True(Server.repo["11"].Count == 2, $"El 11 tiene dos mensajes.");
        }

        [Fact]
        public void Test_ListMessages()
        {
            Server.repo.Clear();
            Message m1 = new Message { From = "22", To = "11", Msg = "Adeu!", Stamp = "A.E." };
            Server.AddMessage(m1);
            Message m2 = new Message { From = "33", To = "11", Msg = "Adios!", Stamp = "A.E." };
            Server.AddMessage(m2);
            Message m3 = new Message { From = "22", To = "33", Msg = "Agur!", Stamp = "A.E." };
            Server.AddMessage(m3);
            Message r1 = Server.ListMessages("11");
            Assert.True(r1.Msg.Contains("[0] From: 22"), $"Primer mensaje de 22 para 11.");
            Assert.True(r1.Msg.Contains("[1] From: 33"), $"Segundo mensaje de 33 para 11.");
            Message r2 = Server.ListMessages("33");
            Assert.True(r2.Msg.Contains("[0] From: 22"), $"Primer mensaje de 22 para 33.");
        }

        [Fact]
        public void Test_RetrMessage()
        {
            Server.repo.Clear();
            Message m1 = new Message { From = "22", To = "11", Msg = "Adeu!", Stamp = "A.E." };
            Server.AddMessage(m1);
            Message m2 = new Message { From = "33", To = "11", Msg = "Adios!", Stamp = "A.E." };
            Server.AddMessage(m2);
            Message m3 = new Message { From = "22", To = "33", Msg = "Agur!", Stamp = "A.E." };
            Server.AddMessage(m3);
            Message m4 = new Message { From = "11", To = "33", Msg = "Bye!", Stamp = "A.E." };
            Server.AddMessage(m4);
            Message r1 = Server.RetrMessage("11", 0);
            Assert.True(r1.Msg.Contains("Adeu!"), $"Primer mensaje para 11.");
            Message r2 = Server.RetrMessage("11", 0);
            Assert.True(r2.Msg.Contains("Adios!"), $"Primer mensaje para 11.");
            Message r3 = Server.RetrMessage("33", 1);
            Assert.True(r3.Msg.Contains("Bye!"), $"Segundo mensaje para 33.");
            Assert.True(Server.repo["33"].Count == 1, $"El 33 tiene un mensaje todavía.");
        }

        [Fact]
        public void Test_Process()
        {
            Message res;
            Server.repo.Clear();
            res = Server.Process(new Message { From = "22", To = "11", Msg = "Adeu!", Stamp = "A.E." });
            res = Server.Process(new Message { From = "33", To = "11", Msg = "Adios!", Stamp = "A.E." });
            res = Server.Process(new Message { From = "22", To = "33", Msg = "Agur!", Stamp = "A.E." });
            res = Server.Process(new Message { From = "11", To = "33", Msg = "Bye!", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("OK"), $"El mensaje de respuesta contiene 'OK'");
            res = Server.Process(new Message { From = "33", To = "0", Msg = "LIST", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("From: 22"), $"El mensaje de respuesta contiene 'From: 22'");
            Assert.True(res.Msg.Contains("From: 11"), $"El mensaje de respuesta contiene 'From: 1'");
            res = Server.Process(new Message { From = "33", To = "0", Msg = "RETR X", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("ERROR"), $"El mensaje de respuesta contiene 'ERROR'");
            res = Server.Process(new Message { From = "33", To = "0", Msg = "RETR -1", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("ERROR"), $"El mensaje de respuesta contiene 'ERROR'");
            res = Server.Process(new Message { From = "33", To = "0", Msg = "RETR 2", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("NOT FOUND"), $"El mensaje de respuesta contiene 'NOT FOUND'");
            res = Server.Process(new Message { From = "33", To = "0", Msg = "RETR 1", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("Bye!"), $"El mensaje de respuesta contiene 'Bye!'");
            res = Server.Process(new Message { From = "44", To = "0", Msg = "RETR 0", Stamp = "A.E." });
            Assert.True(res.Msg.Contains("NOT FOUND"), $"El mensaje de respuesta contiene 'NOT FOUND'");
        }
    }
}
