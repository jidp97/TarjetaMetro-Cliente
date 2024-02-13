using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetroCardSimulatorClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (ClientWebSocket clientWebSocket = new ClientWebSocket())
            {
                await clientWebSocket.ConnectAsync(new Uri("ws://localhost:9090"), CancellationToken.None);

                Console.WriteLine("Conectado al servidor WebSocket.");

                // Iniciar un bucle para recibir actualizaciones del servidor
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer.Array, 0, result.Count);
                        Console.WriteLine($"Actualización del servidor: {receivedMessage}");

                        await Task.Delay(4000); // Esperar 4 segundos antes de la próxima actualización
                    }
                });

                // Bucle principal para enviar solicitudes al servidor
                while (true)
                {
                    Console.Write("\u001b[33mIngrese la acción (Recarga/Consumo): \u001b[0m\n");
                    string action = Console.ReadLine();

                    if (action.ToLower() == "exit")
                        break;

                    if (action.ToLower() == "recarga")
                    {
                        Console.Write("\u001b[33mIngrese el monto de recarga: \u001b[0m\n");
                        string input = Console.ReadLine();

                        if (double.TryParse(input, out double rechargeAmount))
                        {
                            string message = $"{action} {rechargeAmount}";
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
                            await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        else
                        {
                            Console.WriteLine("Entrada no válida. Por favor, ingrese un monto de recarga válido.");
                        }
                    }
                    else if (action.ToLower() == "consumo")
                    {
                        // Consumo fijo por viaje
                        double consumptionAmount = 20;
                        string message = $"{action} {consumptionAmount}";
                        byte[] buffer = Encoding.UTF8.GetBytes(message);
                        await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        Console.WriteLine("Acción no válida. Por favor, ingrese 'Recarga' o 'Consumo'.");
                    }
                }
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando conexión", CancellationToken.None);
            }
        }
    }
}
