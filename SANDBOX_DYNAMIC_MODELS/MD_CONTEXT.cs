using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SANDBOX_DYNAMIC_MODELS
{
    // ===========================================================
    // 2. Contexte EF Core dynamique
    //    ⚠️ À NE PAS IMBRIQUER dans une classe static 
    // ===========================================================
    public class MD_CONTEXT : Microsoft.EntityFrameworkCore.DbContext
    {

        // Dictionnaire interne (modifiable uniquement par cette classe).
        // - clé   : nom logique de l'entité (ex: "MD_CONSTRUCTEUR_AUTO")
        // - valeur: Type CLR généré dynamiquement via Reflection.Emit
        private readonly Dictionary<string, Type> _entities;

        // Propriété publique en lecture seule exposant les entités.
        // L'extérieur peut consulter le dictionnaire mais ne peut pas le modifier.
        // (Encapsulation : protège l'intégrité interne tout en rendant l'accès possible)
        public IReadOnlyDictionary<string, Type> Entities => _entities;

        public MD_CONTEXT(Dictionary<string, Type> entities, DbContextOptions options)
            : base(options)
        {
            _entities = entities;
        }

        /// <summary>
        /// Enregistre les types dynamiques dans le modèle EF Core + configure PK et relations.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ======================================================
            // 1. Enregistrer les entités dynamiques dans le ModelBuilder
            // ======================================================
            foreach (var t in _entities.Values)
                modelBuilder.Entity(t);

            // ======================================================
            // 2. Configurer les PK physiques (ID_Auto auto-incrément)
            // ======================================================
            ConfigurePK(modelBuilder, "MD_CONSTRUCTEUR_AUTO");
            ConfigurePK(modelBuilder, "MD_TYPE_PROPULSION");
            ConfigurePK(modelBuilder, "MD_MODEL_AUTO");

            // ======================================================
            // 3. Configurer les relations basées sur GUIDs métier
            // ======================================================

            // --- Constructeur → Modèles
            // Un constructeur (GUID) peut avoir plusieurs modèles
            modelBuilder.Entity(_entities["MD_CONSTRUCTEUR_AUTO"])
                .HasMany(_entities["MD_MODEL_AUTO"])
                .WithOne()
                .HasForeignKey("ID_Constructeur_FK")     // FK dans MD_MODEL_AUTO
                .HasPrincipalKey("ID_Constructeur");     // GUID logique

            // --- Type de propulsion → Modèles
            // Un type de propulsion (GUID) peut être utilisé par plusieurs modèles
            modelBuilder.Entity(_entities["MD_TYPE_PROPULSION"])
                .HasMany(_entities["MD_MODEL_AUTO"])
                .WithOne()
                .HasForeignKey("ID_TypePropulsion_FK")   // FK dans MD_MODEL_AUTO
                .HasPrincipalKey("ID_TypePropulsion");   // GUID logique
        }

        /// <summary>
        /// Configure une PK auto-incrémentée sur la colonne ID_Auto.
        /// </summary>
        private void ConfigurePK(ModelBuilder mb, string entityName)
        {
            var t = _entities[entityName];
            var eb = mb.Entity(t);

            eb.HasKey("ID_Auto"); // PK physique
            eb.Property("ID_Auto")
              .ValueGeneratedOnAdd()
              .HasAnnotation("Sqlite:Autoincrement", true);
        }

    }
}
