﻿#region Imports

using System;
using System.Collections.ObjectModel;
using System.Linq;
using CrmPluginEntities;

#endregion

namespace CrmPluginRegExt.VSPackage.Model
{
	public class CrmAssembly : CrmEntity<CrmPluginType>
	{
		public bool IsSandbox { get; set; }

		protected override void RunUpdateLogic(XrmServiceContext context)
		{
			var result = (from assembly in context.PluginAssemblySet
			              join type in context.PluginTypeSet
				              on assembly.PluginAssemblyId equals type.PluginAssemblyId.Id
			              where assembly.PluginAssemblyId == Id
			              select new
			                     {
				                     id = assembly.PluginAssemblyId.Value,
				                     name = assembly.Name,
				                     isSandbox = assembly.IsolationMode.Value
				                                 == (int) PluginAssembly.Enums.IsolationMode.Sandbox,
				                     typeId = type.PluginTypeId.Value,
				                     typeName = type.Name,
				                     typeIsWorkflow = type.IsWorkflowActivity.HasValue
				                                      && type.IsWorkflowActivity.Value
			                     }).ToList();

			Children = new ObservableCollection<CrmPluginType>();

			if (!result.Any())
			{
				return;
			}

			Name = result.First().name;
			IsSandbox = result.First().isSandbox;
			Children = new ObservableCollection<CrmPluginType>(result
				.GroupBy(assembly => assembly.typeId)
				.Where(typeGroup => typeGroup.First().id == Id)
				.Select(typeGroup =>
					new CrmPluginType
					{
						Id = typeGroup.First().typeId,
						Name = typeGroup.First().typeName,
						IsWorkflow = typeGroup.First().typeIsWorkflow,
						Assembly = this
					})
				.OrderBy(type => type.IsWorkflow)
				.ThenBy(type => type.Name));
		}

		public void Clear()
		{
			Children = new ObservableCollection<CrmPluginType>();
			IsUpdated = false;
			Name = "";
			Id = Guid.Empty;

			OnUpdate();
		}
	}
}
