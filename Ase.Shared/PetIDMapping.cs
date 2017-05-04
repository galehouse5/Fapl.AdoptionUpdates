namespace Ase.Shared
{
    public struct PetIDMapping
    {
        public string PetPointReferenceNumber { get; set; }
        public int PetfinderID { get; set; }

        public override string ToString()
            => $"{PetPointReferenceNumber} => {PetfinderID}";
    }
}
