using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SANDBOX_DYNAMIC_MODELS
{
    public static class MD_DEMO
    {

        public static void Run()
        {
            var (context, entities) = MD_INIT_BDD.Init();

            InsertAll(context, entities);
            ReadAll(context, entities);
        }

        /// <summary>
        /// Insère 1 constructeur, 1 type propulsion, 1 modèle (avec FKs basés sur GUIDs).
        /// </summary>
        static void InsertAll(MD_CONTEXT context, Dictionary<string, Type> entities)
        {
            // Récupération des types dynamiques générés au runtime
            var tConstructeur = entities["MD_CONSTRUCTEUR_AUTO"];
            var tPropulsion = entities["MD_TYPE_PROPULSION"];
            var tModel = entities["MD_MODEL_AUTO"];

            // ======================================================================
            // 1. Génération des GUIDs métier
            // ======================================================================
            // Ils seront utilisés comme identifiants stables côté code (et comme FK).
            var consGuid = Guid.NewGuid();   // ID_Constructeur
            var propGuid = Guid.NewGuid();   // ID_TypePropulsion

            // ======================================================================
            // 2. Création et initialisation d’un constructeur
            // ======================================================================
            var constructeur = Activator.CreateInstance(tConstructeur)!;
            tConstructeur.GetProperty("ID_Constructeur")!.SetValue(constructeur, consGuid);
            // Note sur l'utilisation du "!" :
            // Quand on écrit "tConstructeur.GetProperty("ID_Constructeur")!.SetValue(constructeur, consGuid);"
            // cela indique : "Je promets au compilateur que ce résultat ne sera jamais null à l’exécution."
            tConstructeur.GetProperty("Nom_Constructeur")!.SetValue(constructeur, "Renault");

            // ======================================================================
            // 3. Création et initialisation d’un type propulsion
            // ======================================================================
            var propulsion = Activator.CreateInstance(tPropulsion)!;
            tPropulsion.GetProperty("ID_TypePropulsion")!.SetValue(propulsion, propGuid);
            tPropulsion.GetProperty("Description")!.SetValue(propulsion, "Électrique");
            tPropulsion.GetProperty("BL_ESS")!.SetValue(propulsion, false);
            tPropulsion.GetProperty("BL_DIE")!.SetValue(propulsion, false);
            tPropulsion.GetProperty("BL_HYB")!.SetValue(propulsion, false);
            tPropulsion.GetProperty("BL_REC")!.SetValue(propulsion, true);

            // ======================================================================
            // 4. Création et initialisation d’un modèle auto
            // ======================================================================
            var model = Activator.CreateInstance(tModel)!;
            tModel.GetProperty("ID_Model")!.SetValue(model, 200);   // simple ID métier (int)
            tModel.GetProperty("Nom_Model")!.SetValue(model, "Zoé");

            // FK → valeurs GUID qu’on a générées plus haut
            tModel.GetProperty("ID_Constructeur_FK")!.SetValue(model, consGuid);
            tModel.GetProperty("ID_TypePropulsion_FK")!.SetValue(model, propGuid);

            // ======================================================================
            // 5. Ajout au contexte EF Core
            // ======================================================================
            context.Add(constructeur);
            context.Add(propulsion);
            context.Add(model);

            // Enregistrement en BDD SQLite
            context.SaveChanges();

            Console.WriteLine("✅ Données insérées avec GUIDs (Constructeur + Propulsion + Modèle).");
        }

        /// <summary>
        /// Lecture : utilise DbContext.Set(Type) (EF Core 9) → DbSet non générique.
        /// </summary>
        static void ReadAll(MD_CONTEXT context, Dictionary<string, Type> entities)
        {
            var tConstructeur = entities["MD_CONSTRUCTEUR_AUTO"];
            var tPropulsion = entities["MD_TYPE_PROPULSION"];
            var tModel = entities["MD_MODEL_AUTO"];

            Console.WriteLine("\n📌 Lecture des données :");

            // --- Constructeurs ---
            foreach (var c in GetSet(context, tConstructeur))
            {
                var idMetier = tConstructeur.GetProperty("ID_Constructeur")!.GetValue(c);
                var nom = tConstructeur.GetProperty("Nom_Constructeur")!.GetValue(c);
                Console.WriteLine($"Constructeur  → ID_Constructeur={idMetier}, Nom={nom}");
            }

            // --- Types propulsion ---
            foreach (var p in GetSet(context, tPropulsion))
            {
                var idMetier = tPropulsion.GetProperty("ID_TypePropulsion")!.GetValue(p);
                var desc = tPropulsion.GetProperty("Description")!.GetValue(p);
                Console.WriteLine($"Propulsion    → ID_TypePropulsion={idMetier}, Description={desc}");
            }

            // --- Modèles ---
            foreach (var m in GetSet(context, tModel))
            {
                var nom = tModel.GetProperty("Nom_Model")!.GetValue(m);
                var fkCons = tModel.GetProperty("ID_Constructeur_FK")!.GetValue(m);
                var fkProp = tModel.GetProperty("ID_TypePropulsion_FK")!.GetValue(m);
                Console.WriteLine($"Modèle        → Nom={nom}, FK_Constructeur={fkCons}, FK_Propulsion={fkProp}");
            }
        }

        // Helper ultra-robuste : appelle DbContext.Set<TEntity>() par réflexion
        private static IEnumerable GetSet(Microsoft.EntityFrameworkCore.DbContext ctx, Type entityType)
        {
            // On cible la méthode générique Set<TEntity>() de Microsoft.EntityFrameworkCore.DbContext
            var setMethod = typeof(Microsoft.EntityFrameworkCore.DbContext)
                .GetMethod(nameof(Microsoft.EntityFrameworkCore.DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(entityType);

            // setMethod.Invoke(...) retourne un DbSet<TEntity> qui implémente IEnumerable
            return (IEnumerable)setMethod.Invoke(ctx, null)!;
        }

    }
}
