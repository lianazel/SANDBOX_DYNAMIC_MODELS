using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SANDBOX_DYNAMIC_MODELS
{
    // ===========================================================
    // 4) Orchestration initialisation BDD
    // ===========================================================
    public class MD_INIT_BDD
    {

        /// <summary>
        /// Crée les modèles, le contexte, (re)crée la BDD et retourne le tout.
        /// </summary>
        public static (MD_CONTEXT context, Dictionary<string, Type> entities) Init()
        {
            var entities = MD_INIT_MODELS.CreateModels();
            var context = MD_INIT_EFCORE.CreateContext(entities);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            Console.WriteLine("✅ SQLite (re)créée.");
            return (context, entities);
        }

    }
}
