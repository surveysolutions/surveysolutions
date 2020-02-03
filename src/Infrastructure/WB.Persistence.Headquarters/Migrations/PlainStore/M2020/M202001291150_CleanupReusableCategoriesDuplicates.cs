using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202001291150)]
    public class M202001291150_CleanupReusableCategoriesDuplicates : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"DELETE  FROM plainstore.reusablecategoricaloptions T1
                  USING       plainstore.reusablecategoricaloptions T2
                WHERE  T1.ctid < T2.ctid       
                  AND  T1.questionnaireid      = T2.questionnaireid 
                  AND  T1.questionnaireversion = T2.questionnaireversion
                  AND  T1.categoriesid         = T2.categoriesid
                  AND  T1.sortindex            = T2.sortindex
                  AND  COALESCE(T1.parentvalue, -9999997) = COALESCE(T2.parentvalue, -9999997)
                  AND  T1.value                = T2.value  
                  AND  T1.text                 = T2.text");
        }

        public override void Down()
        {
            
        }
    }
}
