using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace SANDBOX_DYNAMIC_MODELS
{
    // ===========================================================
    // 1. Génération des entités dynamiques
    // ===========================================================
    public static class MD_INIT_MODELS
    {
        /// <summary>
        /// Crée dynamiquement les 3 modèles de données demandés
        /// (Constructeur, Modèle, TypePropulsion).
        /// </summary>
        public static Dictionary<string, Type> CreateModels()
        {
            // > Le "nom logique" de l’assembly que nous allons générer <
            // [ comme si on avait créé un .dll, mais ici tout reste en mémoire ]
            var assemblyName = new AssemblyName("SANDBOX_DYNAMIC_MODELS.DynamicAssembly");

            // > Un AssemblyBuilder permet de construire un assembly au runtime <
            // [ AssemblyBuilderAccess.Run = uniquement exécution (pas sauvegarde .dll) ]
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            // > Un Assembly peut contenir plusieurs "modules" <
            // Un module est une unité de compilation (un .dll peut contenir plusieurs modules, ...
            // ...mais dans .NET moderne on en met quasi toujours 1 seul)
            // [ Ici on définit le module principal dans lequel on créera nos classes ]
            var module = assemblyBuilder.DefineDynamicModule("MainModule");

            
            // > Dictionnaire pour stocker les Types générés <
            // La clé = nom logique (ex: "MD_CONSTRUCTEUR_AUTO")
            // [ La valeur = le Type CLR généré dynamiquement (ex: Type représentant la classe runtime) ]
            var entities = new Dictionary<string, Type>();

            // ==============================================================
            // MD_CONSTRUCTEUR_AUTO
            // ==============================================================
            entities["MD_CONSTRUCTEUR_AUTO"] = CreateEntity(module, "MD_CONSTRUCTEUR_AUTO", new()
            {
                { "ID_Auto", typeof(int) },                  // PK autoincrémentée
                { "ID_Constructeur", typeof(Guid) },         // GUID métier
                { "Nom_Constructeur", typeof(string) }
            });

            // ==============================================================
            // MD_TYPE_PROPULSION
            // ==============================================================
            entities["MD_TYPE_PROPULSION"] = CreateEntity(module, "MD_TYPE_PROPULSION", new()
            {
                { "ID_Auto", typeof(int) },                  // PK autoincrémentée
                { "ID_TypePropulsion", typeof(Guid) },       // GUID métier
                { "Description", typeof(string) },
                { "BL_ESS", typeof(bool) },
                { "BL_DIE", typeof(bool) },
                { "BL_HYB", typeof(bool) },
                { "BL_REC", typeof(bool) },
            });

            // ==============================================================
            // MD_MODEL_AUTO
            // ==============================================================
            entities["MD_MODEL_AUTO"] = CreateEntity(module, "MD_MODEL_AUTO", new()
            {
                { "ID_Auto", typeof(int) },                  // PK autoincrémentée
                { "ID_Model", typeof(int) },                 // ID métier (ici int simple)
                { "ID_Constructeur_FK", typeof(Guid) },      // FK → ID_Constructeur
                { "Nom_Model", typeof(string) },
                { "ID_TypePropulsion_FK", typeof(Guid) }     // FK → ID_TypePropulsion
            });

            return entities;
        }


        /// <summary>
        /// Crée un type dynamique avec ses propriétés (auto-implémentées via IL).
        /// </summary>
        private static Type CreateEntity(ModuleBuilder module, string name, Dictionary<string, Type> props)
        {
            // On crée un "type builder" pour générer une classe publique (TypeAttributes.Public)
            var typeBuilder = module.DefineType(name, TypeAttributes.Public | TypeAttributes.Class);

            foreach (var kvp in props)
            {
                // ---------------------------------------------------
                // 1. Définir un champ privé pour stocker la valeur
                // ---------------------------------------------------
                // -> Explication sur "DefineField($"_{kvp.Key}", kvp.Value, FieldAttributes.Private)" 
                // -> signifie en fait "private <Type> _NomPropriete;"
                // -> Exemple : cela crée dans le type généré :  "private string _Nom_Constructeur;"
                var field = typeBuilder.DefineField($"_{kvp.Key}", kvp.Value, FieldAttributes.Private);

                // ---------------------------------------------------
                // 2. Définir une propriété publique avec le même nom
                // ---------------------------------------------------
                var prop = typeBuilder.DefineProperty(kvp.Key, PropertyAttributes.HasDefault, kvp.Value, null);

                // ---------------------------------------------------
                // 3. Générer la méthode GET (public <Type> get_<Nom>())
                // ---------------------------------------------------
                // -> Rappel : Le "|" est un "OU" binaire : il permet de combiner plusieurs flags d’énumération.
                var getter = typeBuilder.DefineMethod(
                    $"get_{kvp.Key}",                               // nom méthode : get_Propriete
                    MethodAttributes.Public                         // méthode publique
                    | MethodAttributes.SpecialName                  // marque comme "spéciale" (propriété, opérateur…)
                    | MethodAttributes.HideBySig,                   // cache les surcharges avec même signature
                    kvp.Value,                                      // type de retour (ex: string, int, Guid)
                    Type.EmptyTypes);                               // pas de paramètre

                var getterIL = getter.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);        // charger "this"
                getterIL.Emit(OpCodes.Ldfld, field);   // charger la valeur du champ privé
                getterIL.Emit(OpCodes.Ret);            // retourner la valeur

                // ---------------------------------------------------
                // 4. Générer la méthode SET (public void set_<Nom>(T value))
                // ---------------------------------------------------
                var setter = typeBuilder.DefineMethod(
                    $"set_{kvp.Key}",                                     // nom méthode : set_Propriete
                    MethodAttributes.Public
                    | MethodAttributes.SpecialName
                    | MethodAttributes.HideBySig,
                    null,                                                 // pas de retour
                    new[] { kvp.Value });                                 // paramètre = type de la propriété

                var setterIL = setter.GetILGenerator();
                setterIL.Emit(OpCodes.Ldarg_0);      // charger "this"
                setterIL.Emit(OpCodes.Ldarg_1);      // charger "value"
                setterIL.Emit(OpCodes.Stfld, field); // stocker dans le champ privé
                setterIL.Emit(OpCodes.Ret);

                // ---------------------------------------------------
                // 5. Relier la propriété aux méthodes getter/setter
                // ---------------------------------------------------
                prop.SetGetMethod(getter);
                prop.SetSetMethod(setter);
            }

            // Crée et retourne le Type final
            return typeBuilder.CreateType()!;
        }
    }
}
