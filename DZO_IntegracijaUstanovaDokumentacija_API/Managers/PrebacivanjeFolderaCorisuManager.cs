using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFolderaCorisuManager
    {
        public void PrebaciFoldere()
        {
                //List<DZOI_VratiFajloveZaPrebacivanjeResult> fajlovi = _db.Procedures.DZOI_VratiFajloveZaPrebacivanjeAsync().Result.ToList();

                //foreach (var item in fajlovi)
                //{
                //    string brUputa = new string(item.UputBroj);
                //    int IdJson = Convert.ToInt32(item.IdJson);
                //    int idSpec = Convert.ToInt32(item.IdSpec);
                //    string NazivFajla = new string(item.NazivFajla);
                //    Logovi logovi = new(_db);
                //    try
                //    {
                //        var uspeh = _sftpService.PrebaciFajlove(brUputa, NazivFajla);
                //        if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "fajl ne postoji"); logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3); }
                //        else { logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 2); }
                //    }
                //    catch (Exception ex)
                //    {

                //        logovi.LogError(IdJson, "desila se greska prilikom prebacivanja fajla:" + ex);
                //        logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3);
                //    }

                //}

        }
    }
}
