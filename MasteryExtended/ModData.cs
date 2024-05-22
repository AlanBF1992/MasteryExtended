namespace MasteryExtended
{
    public class ModData
    {
        public int claimedRewards { get; set; } = 0;

        /* Notes for myself
         *
         *  To get the multiplayer farmers
         *  IEnumerable<Farmer> allFarmhands = Game1.getAllFarmers().Where(f => !f.IsLocalPlayer && !String.IsNullOrEmpty(f.Name));
         *  Don't needed?
         *
         *  To check if they are connected
         *  bool connected = Game1.otherFarmers.ContainsKey(farmhand.UniqueMultiplayerID);
         *  Don't needed?
         *
         *  To access them
         *  Game1.getFarmerMaybeOffline(farmhand.UniqueMultiplayerID); // Null if they are not connected
         *  Don't needed?
         *
         *  Para guardar los datos de los farmhands debería hacerlo automáticamente al dictionary del main player cuando lo hagan
         *  Esto enviando un mensaje desde el IsLocalPlayer == false y recibiendo desde el IsLocalPlayer = true
         *  Recordar agregar el nuevo farmhand si no existe en el diccionario
         *
         *  Diccionario
         *  public List<Dictionary<long, Dictionary<string, int>>> farmhands { get; set; } = new();
         *  long = farmhand UniqueMultiplayerID
         *  -> string = property
         *  -> int = value
         *  En general van a ser bools, pero just in case, así sirve para valores y categorias
         */
    }
}
