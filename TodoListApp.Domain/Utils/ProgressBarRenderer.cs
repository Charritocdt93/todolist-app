namespace TodoListApp.Domain.Utils
{
    /// <summary>
    /// Clase auxiliar para renderizar una barra de progreso de longitud fija (50 caracteres):
    /// llena con 'O' hasta el porcentaje indicado y deja espacios para lo que reste.
    /// </summary>
    public static class ProgressBarRenderer
    {
        public static string Render(decimal percent)
        {
            const int totalBlocks = 50;
            // Calculamos cuántos bloques llenos (redondeo al entero más cercano)
            int filledBlocks = (int)Math.Round((percent / 100m) * totalBlocks);

            if (filledBlocks < 0) filledBlocks = 0;
            if (filledBlocks > totalBlocks) filledBlocks = totalBlocks;

            string filled = new string('O', filledBlocks);
            string empty = new string(' ', totalBlocks - filledBlocks);
            return $"|{filled}{empty}|";
        }
    }
}
