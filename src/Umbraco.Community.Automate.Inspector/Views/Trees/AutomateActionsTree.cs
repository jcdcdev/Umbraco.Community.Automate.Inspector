using JetBrains.Annotations;
using Umbraco.Automate.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Community.SimpleTrees.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Community.Automate.Inspector.Views.Trees;

public class AutomateMenu : SimpleMenu
{
    public override string Name => "Documentation";
    public override string[] Sections => ["Ua.Section.Automate"];
}

[UsedImplicitly]
public class AutomateActionsTree(ActionCollection actionCollection, ISimpleTreeContext context) : SimpleTree(context)
{
    public override string[] Menus => [nameof(AutomateMenu)];
    public override int Weight => 0;

    public async override Task<PagedModel<ISimpleTreeItem>> GetTreeRootAsync(int skip, int take, bool foldersOnly)
    {
        var groupedActions = actionCollection.Select(x => x.Group)
            .WhereNotNull()
            .Distinct()
            .OrderBy(x => x);

        var data = groupedActions.Select(x => CreateRootItem(x, x, hasChildren: true)).ToList();
        return new PagedModel<ISimpleTreeItem>(data.Count, data);
    }

    public async override Task<PagedModel<ISimpleTreeItem>> GetTreeChildrenAsync(string entityType, string parentUnique, int skip, int take, bool foldersOnly)
    {
        var actions = actionCollection.Where(x => x.Group == parentUnique).ToList();
        var total = actions.Count;
        var data = actions.Select(x => CreateItem(x.Name, x.Alias, parentUnique, x.Icon ?? "icon-document", isFolder: false, hasChildren: false))
            .Skip(skip)
            .Take(take)
            .ToList();

        return new PagedModel<ISimpleTreeItem>(total, data);
    }

    public override string Name => "Actions";
}