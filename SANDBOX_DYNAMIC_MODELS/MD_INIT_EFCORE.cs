using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SANDBOX_DYNAMIC_MODELS
{
    public static class MD_INIT_EFCORE
    {

        /// <summary>
        /// Création du DbContext EF Core basé sur nos entités dynamiques.
        /// </summary>
        public static MD_CONTEXT CreateContext(Dictionary<string, Type> entities)
        {
            var options = new DbContextOptionsBuilder<MD_CONTEXT>()
                .UseSqlite("Data Source=SandboxDynamicModels.db")
                .Options;


            // - - - - - - - - - - - - - - - - - - - - - 
            // !! Attention !!
            //  On ne délcare PAS de type "object" en retour car on perdrait...
            //    ... tout l'intêrer d'avoir un vrai context.
            //    
            //  On retournant un MD_CONTEXT, on renvoie effectivement une instance du contexte EF Core 
            // - - - - - - - - - - - - - - - - - - - - - 

            return new MD_CONTEXT(entities, options);
        }

    }
}
