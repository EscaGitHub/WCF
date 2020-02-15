using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Infrastructure
{
    public class QueryBuilder
    {
        public string ChangedDocumentsByVariableQuery(CatalogDefinition catalogDefinition, string[] systemAttributes)
        {
            var builder = new StringBuilder();

            BuildVariableDeclarations(catalogDefinition.StatusVariableName, systemAttributes, builder);

            builder.AppendLine(@"select
docs.DocumentID as N'Идентификатор документа'
,case when revisions.RevNr is null then 0 else revisions.RevNr end as N'Версия документа'
,docConfigs.ConfigurationName as N'Имя конфигурации документа'
,revisions.Date as N'Дата создания версии'");

            BuildSelectVariableTextValue(catalogDefinition.StatusVariableName, systemAttributes, builder);

            builder.AppendLine(@"from DocumentRevisionConfiguration
inner join DocumentConfiguration as docConfigs on docConfigs.ConfigurationID = DocumentRevisionConfiguration.ConfigurationID
inner join Documents as docs on docs.DocumentID = DocumentRevisionConfiguration.DocumentID
inner join Revisions as revisions on revisions.RevNr = DocumentRevisionConfiguration.RevisionNo and revisions.DocumentID = docs.DocumentID
                and revisions.RevNr = (select max(UserRevs.RevNr) from UserRevs where DocumentID = docs.DocumentId)");

            BuildProductLeftJoinVariables(systemAttributes, builder);

            BuildFolderPathDeclarations(catalogDefinition.FolderDefinitions, builder);

            builder.AppendLine($@"where
StatusVariableValues.ValueText = N'{catalogDefinition.StatusVariableValue}' and folders.TruePath = 1");

            return builder.ToString().TrimEnd('\r', '\n');
        }

        private void BuildProductLeftJoinVariables(IEnumerable<string> systemAttributes, StringBuilder builder)
        {
            var i = 0;
            foreach (var variableDefinition in systemAttributes)
            {
                i++;
                builder.AppendLine($@"left join VariableValue as Variable{i}Values on Variable{i}Values.DocumentID = docs.DocumentId
and Variable{i}Values.ConfigurationID = docConfigs.ConfigurationID
and Variable{i}Values.VariableID = @variableId{i} 
and Variable{i}Values.RevisionNo = (select max(RevisionNo) from VariableValue where DocumentID = docs.DocumentId 
                                        and ConfigurationID = docConfigs.ConfigurationID
										and VariableID = @variableId{i}
										and RevisionNo <= revisions.RevNr)");
            }

            builder.AppendLine(@"left join VariableValue as StatusVariableValues on StatusVariableValues.DocumentID = docs.DocumentId
and StatusVariableValues.ConfigurationID = docConfigs.ConfigurationID
and StatusVariableValues.VariableID = @statusVariableId 
and StatusVariableValues.RevisionNo = (select max(RevisionNo) from VariableValue where DocumentID = docs.DocumentId 
                                        and ConfigurationID = docConfigs.ConfigurationID
										and VariableID = @statusVariableId
										and RevisionNo <= revisions.RevNr)");

        }

        private void BuildSelectVariableTextValue(string statusVariableName, string[] systemAttributes, StringBuilder builder)
        {
            var i = 0;
            foreach (var variableDefinition in systemAttributes)
            {
                i++;
                builder.AppendLine($",Variable{i}Values.ValueText as N'{variableDefinition}'");
            }

            builder.AppendLine($",StatusVariableValues.ValueText as N'{statusVariableName}'");

            builder.AppendLine($",cast((case when revisions.RevNr is null then 0 else revisions.RevNr end) as nvarchar(max)) as N'{Properties.Resources.VersionNumberColumn}'");

            builder.AppendLine($",folders.Path as N'{Properties.Resources.FolderPathColumn}'");

            builder.AppendLine($",docs.FileName as N'{Properties.Resources.FileNameColumn}'");
        }

        private void BuildFolderPathDeclarations(IReadOnlyCollection<FolderDefinition> catalogDefinitions, StringBuilder builder)
        {
            builder.AppendLine(@"cross apply (select top 1
DocumentID
,Path");
            if (catalogDefinitions.Count == 0)
            {
                builder.AppendLine(@",1 as TruePath");
            }
            else
            {
                var i = 0;

                var paths = catalogDefinitions.SelectMany(t => t.FolderPaths).Distinct();

                foreach (var catalogPath in paths)
                {
                    var path = catalogPath;

                    if (!path.EndsWith("\\")) path += "\\";

                    i++;

                    builder.AppendLine(i == 1
                        ? $@",CASE WHEN folders.Path like('{path}%') "
                        : $@"OR folders.Path like('{path}%') ");
                }

                builder.AppendLine(@"THEN 1 ELSE 0 END as TruePath");
            }

            builder.AppendLine(@"from DocumentsInProjects as docsInProject 
inner join Projects as folders on folders.ProjectID = docsInProject.ProjectID
where docsInProject.Deleted != 1 and docsInProject.DocumentID = docs.DocumentID
order by TruePath desc) as folders");
        }

        private void BuildVariableDeclarations(string statusVariable, IEnumerable<string> systemAttributes, StringBuilder builder)
        {
            var i = 0;
            foreach (var variableName in systemAttributes)
            {
                i++;
                builder.AppendLine($"declare @variableId{i} as int = (select VariableID from Variable where VariableName = N'{variableName}' and IsDeleted = 0);");
            }

            builder.AppendLine($"declare @statusVariableId as int = (select VariableID from Variable where VariableName = N'{statusVariable}' and IsDeleted = 0);");
        }
    }
}
