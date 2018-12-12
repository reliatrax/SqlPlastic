using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    class AssociationBuilder
    {
        PlasticConfig Config;

        Dictionary<int, HashSet<string>> tablePropertyNames = new Dictionary<int, HashSet<string>>();

        public AssociationBuilder( PlasticConfig config )
        {
            Config = config;
        }

        public void AddAssociationProperties( IEnumerable<Table> tables, IEnumerable<ForeignKeyDescriptor> fkds )
        {
            var colDict = tables.SelectMany(x => x.Columns).ToDictionary(x => x.ColumnUID);

            foreach (var fkd in fkds)
            {
                Column c1 = colDict[new ColumnUID(fkd.ParentObjectID, fkd.ParentColumnID)];
                Column c2 = colDict[new ColumnUID(fkd.ReferencedObjectID, fkd.ReferencedColumnID)];

                AddAssociationPropsForKey(fkd, c1, c2);
            }
        }

        private void AddAssociationPropsForKey(ForeignKeyDescriptor fkd, Column c1, Column c2)
        {
            //  Table 1                  Table 2
            //  dbo.Orders               dbo.Customers
            //  MyCustomerID INT  --->   CustomerID INT 
            //    (foreign key)              (primary key)
            //
            //  Table 1 gets a property EntityRef<Customer> Customer
            //  Table 2 gets a collection EntitySet<Order>  Orders

            Table t1 = c1.Table, t2 = c2.Table;

            bool isOneToOne = false; //#TODO fix one-to-one relationships c1.IsPrimaryKey;

            // Generate property names
            FkProps fkProps = GenerateProps(c1, t2, fkd);

            // --- Add an EntityRef from c1 ---> c2
            var eref = new EntityRefModel
            {
                EntityRefName = fkProps.entityRefName,
                KeyColumn = c1,                 // T1 is the *ref*
                ReferencedColumn = c2,          // T2 is the *set*

                ForeignKeyName = fkd.ForeignKeyName,
                DeleteRule = fkd.OnDelete,
                DeleteOnNull = fkProps.DeleteOnNull ?"true" :"false"
            };

            // Set the Foreign Key Column to point to this foreign key
            c1.ForeignKey = eref;
            t1.EntityRefs.Add(eref);    // Add to the tables

            if (isOneToOne)
            {
                // --- For one-to-one relationships, Add an entity ref to t2 pointing back to t1
                var eref2 = new EntityRefModel
                {
                    EntityRefName = fkProps.entityRefName,
                    KeyColumn = c2,                 // T2 is the *ref2*
                    ReferencedColumn = c1,          // T1 is the *ref*

                    ForeignKeyName = fkd.ForeignKeyName,
                    DeleteRule = fkd.OnDelete,
                    DeleteOnNull = fkProps.DeleteOnNull ? "true" : "false"
                };

                // Set the cross-linking association properties
                t2.EntityRefs.Add(eref2);        // Add to the tables
            }
            else
            {
                // --- For one-to-many relationships, Add an EntitySet from c1 <-- c2
                var eset = new EntitySetModel
                {
                    EntitySetName = fkProps.entitySetName,
                    KeyColumn = c2,                 // T2 is the *set*
                    ReferencedColumn = c1,          // T1 is the *ref*

                    ForeignKeyName = fkd.ForeignKeyName,
                    DeleteRule = fkd.OnDelete,
                };

                // Set the cross-linking association properties
                eref.AssociatedSet = eset;
                eset.AssociatedRef = eref;
                t2.EntitySets.Add(eset);        // Add to the tables
            }

        }

        class FkProps
        {
            public string entityRefName { get; set; }
            public string entitySetName { get; set; }
            public bool DeleteOnNull { get; set; }
        }

        FkProps GenerateProps(Column c1, Table t2, ForeignKeyDescriptor fkd)
        {
            // lookup any table-specific mapping rules
            TableMappingRules mappingRules = new TableMappingRules();

            Table t1 = c1.Table;

            string fullTableName = t1.SchemaName + "." + t1.TableName;      // Foreign keys are "owned" by table 1
            if (Config.MappingRules.TryGetValue(fullTableName, out TableMappingRules mr))
                mappingRules = mr;

            var fkm = mappingRules.ForeignKeys.FirstOrDefault(x => x.ForeignKeyName == fkd.ForeignKeyName);

            string entityRefName, entitySetName;

            // --- Entity Ref Name
            if( string.IsNullOrEmpty(fkm?.EntityRefName) )
            {
                string refName = ModelBuilder.GenerateClassName(t2.TableName);
                entityRefName = GeneratePropertyName(t1, refName);
            }
            else
            {
                entityRefName = fkm.EntityRefName;
            }

            // --- Entity Set Name
            if (string.IsNullOrEmpty(fkm?.EntitySetName))
            {
                string className = ModelBuilder.GenerateClassName(t1.TableName);
                string setName = Inflector.Pluralize(className);        // the set name should be plural
                entitySetName = GeneratePropertyName(t2, setName);
            }
            else
            {
                entitySetName = fkm.EntitySetName;
            }

            // --- Delete on Null
            bool deleteOnNull = fkm?.DeleteOnNull ?? (c1.IsNullable == false);     // If a non-nullable foreign key is set to null, we need to delete the associated record 


            return new FkProps
            {
                entityRefName = entityRefName,
                entitySetName = entitySetName,
                DeleteOnNull = deleteOnNull 
            };
        }


        string GeneratePropertyName(Table tbl, string baseName)
        {
            // Lazy initialize the dictionary of HashSets
            if( tablePropertyNames.TryGetValue(tbl.TableObjectID, out HashSet<string> propNames) == false )
            {
                propNames = new HashSet<string>(tbl.Columns.Select(x => x.MemberName));
                propNames.Add(tbl.ClassName);       // Add the classname itself, since classes cannot have a property name the same as the enclosing class name
                tablePropertyNames.Add(tbl.TableObjectID, propNames);
            }

            // First check any user-specified mapping rules


            // Next check the basename -- if it doesn't exist, then use it
            if (propNames.Contains(baseName) == false)
            {
                propNames.Add(baseName);
                return baseName;
            }

            // This would be a great place for an extension point.  Maybe a text file list of table / property names?

            for (int i = 1; i < 10; ++i)
            {
                string name = baseName + i.ToString();
                if (propNames.Contains(name) == false)
                {
                    propNames.Add(name);
                    return name;
                }
            }

            throw new ArgumentException($"Unable to find unique name for {tbl.SchemaName}.{tbl.TableName}.{baseName}");
        }

    }
}
