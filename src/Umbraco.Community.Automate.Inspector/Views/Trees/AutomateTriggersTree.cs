using JetBrains.Annotations;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Models;
using Umbraco.Community.SimpleTrees.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Community.Automate.Inspector.Views.Trees;

[UsedImplicitly]
public class AutomateTriggersTree(TriggerCollection triggerCollection, ISimpleTreeContext context) : SimpleTree(context)
{
    public override int Weight => 1;
    public override string[] Menus => [nameof(AutomateMenu)];

    public async override Task<PagedModel<ISimpleTreeItem>> GetTreeRootAsync(int skip, int take, bool foldersOnly)
    {
        var groupedActions = triggerCollection.Select(x => x.Group)
            .WhereNotNull()
            .Distinct()
            .OrderBy(x => x);

        var data = groupedActions.Select(x => CreateRootItem(x, x, hasChildren: true)).ToList();
        return new PagedModel<ISimpleTreeItem>(data.Count, data);
    }

    public async override Task<PagedModel<ISimpleTreeItem>> GetTreeChildrenAsync(string entityType, string parentUnique, int skip, int take, bool foldersOnly)
    {
        var actions = triggerCollection.Where(x => x.Group == parentUnique).ToList();
        var total = actions.Count;
        var data = actions.Select(x => CreateItem(x.Name, x.Alias, parentUnique, x.Icon ?? "icon-document", isFolder: false, hasChildren: false))
            .Skip(skip)
            .Take(take)
            .ToList();

        return new PagedModel<ISimpleTreeItem>(total, data);
    }

    public override string Name => "Triggers";
}