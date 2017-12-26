using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlPlastic
{
    static class AssociationBuilder
    {
        static Dictionary<int, HashSet<string>> tablePropertyNames = new Dictionary<int, HashSet<string>>();

        public static void AddAssociationProperties( IEnumerable<Table> tables, IEnumerable<ForeignKeyDescriptor> fkds )
        {
            var colDict = tables.SelectMany(x => x.Columns).ToDictionary(x => x.ColumnUID);

            foreach (var fkd in fkds)
            {
                Column c1 = colDict[new ColumnUID(fkd.ParentObjectID, fkd.ParentColumnID)];
                Column c2 = colDict[new ColumnUID(fkd.ReferencedObjectID, fkd.ReferencedColumnID)];

                AddAssociationPropsForKey(fkd, c1, c2);
            }
        }

        private static void AddAssociationPropsForKey(ForeignKeyDescriptor fkd, Column c1, Column c2)
        {
            //  Table 1                  Table 2
            //  dbo.Orders               dbo.Customers
            //  MyCustomerID INT  --->   CustomerID INT 
            //
            //  Table 1 gets a property EntityRef<Customer> Customer
            //  Table 2 gets a collection EntitySet<Order>  Orders

            Table t1 = c1.Table, t2 = c2.Table;

            // --- Add an EntityRef from c1 ---> c2
            string refName = Inflector.Singularize(t2.TableName);      // #TODO - Mapper!!! PropertyName from TableName
            refName = GeneratePropertyName(t1, refName );

            var eref = new EntityRefModel
            {
                EntityRefName = refName,
                KeyColumn = c1,                 // T1 is the *ref*
                ReferencedColumn = c2,          // T2 is the *set*

                ForeignKeyName = fkd.ForeignKeyName,
                DeleteRule = fkd.OnDelete,
            };

            // --- Add an EntitySet from c1 <-- c2
            string setName = t1.TableName;      // #TODO - Mapper!!! PropertyName from TableName
            setName = GeneratePropertyName(t2, setName );

            var eset = new EntitySetModel
            {
                EntitySetName = setName,
                KeyColumn = c2,                 // T2 is the *set*
                ReferencedColumn = c1,          // T1 is the *ref*

                ForeignKeyName = fkd.ForeignKeyName,
                DeleteRule = fkd.OnDelete,
            };

            // Set the cross-linking association properties
            eref.AssociatedSet = eset;
            eset.AssociatedRef = eref;

            // Add to the tables
            t1.EntityRefs.Add(eref);
            t2.EntitySets.Add(eset);
        }


        private static string GeneratePropertyName(Table tbl, string baseName)
        {
            // Lazy initialize the dictionary of HashSets
            if( tablePropertyNames.TryGetValue(tbl.ObjectID, out HashSet<string> propNames) == false )
            {
                propNames = new HashSet<string>(tbl.Columns.Select(x => x.MemberName));
                tablePropertyNames.Add(tbl.ObjectID, propNames);
            }

            // First check the basename
            if (propNames.Contains(baseName) == false)
            {
                propNames.Add(baseName);
                return baseName;
            }

            // This would be a great place for an extension point.  Maybe a text file list of table / property names?

            for (int i = 0; i < 10; ++i)
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
