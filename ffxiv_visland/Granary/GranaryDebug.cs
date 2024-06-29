﻿using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using System;
using visland.Helpers;

namespace visland.Granary;

public unsafe class GranaryDebug
{
    private UITree _tree = new();

    public void Draw()
    {
        var granary = MJIManager.Instance()->GranariesState;
        foreach (var n in _tree.Node($"Granaries state: {(nint)granary:X}"))
        {
            if (granary != null)
            {
                DrawGranaryState(1, ref granary->Granary[0]);
                DrawGranaryState(2, ref granary->Granary[1]);
            }
        }

        var agent = AgentMJIGatheringHouse.Instance();
        foreach (var n in _tree.Node($"Agent: {(nint)agent:X}"))
        {
            if (agent != null)
            {
                _tree.LeafNode($"Granaries state: {(nint)agent->GranariesState:X}");
                _tree.LeafNode($"Confirm: addon={agent->ConfirmAddonHandle} #{agent->ConfirmType} '{agent->Strings.ConfirmText}'");
                _tree.LeafNode($"Select: addon={agent->SelectExpeditionAddonHandle}, return='{agent->Strings.FinishTimeText[0]}'/'{agent->Strings.FinishTimeText[1]}'");
                _tree.LeafNode($"Selected granary={agent->CurGranaryIndex}");
                _tree.LeafNode($"Selected expedition={agent->CurActiveExpeditionId} {agent->CurExpeditionName}, hover={agent->CurHoveredExpeditionId}, proposed={agent->CurSelectedExpeditionId}");
                _tree.LeafNode($"Selected days={agent->CurActiveDays} -> {agent->CurSelectedDays}");
                if (agent->Data != null)
                {
                    _tree.LeafNode($"Agent data inited: {agent->Data->Initialized}");
                    foreach (var m in _tree.Node("Expeditions", agent->Data->Expeditions.Size() == 0))
                    {
                        int i = 0;
                        foreach (var e in agent->Data->Expeditions.Span)
                        {
                            foreach (var k in _tree.Node($"[{i++}] {e.ExpeditionId} '{e.Name}'"))
                            {
                                _tree.LeafNode($"[rare] {e.RareItemId} '{Service.LuminaRow<Item>(e.RareItemId)?.Name}' {e.RareIconId}");
                                for (int j = 0; j < e.NumNormalItems; ++j)
                                    _tree.LeafNode($"[{j}] {e.NormalItemIds[j]} '{Service.LuminaRow<Item>(e.NormalItemIds[j])?.Name}' {e.NormalIconIds[j]}");
                            }
                        }
                    }
                    foreach (var m in _tree.Node("Expedition Descs", agent->Data->ExpeditionDescs.Size() == 0))
                    {
                        int i = 0;
                        foreach (var e in agent->Data->ExpeditionDescs.Span)
                        {
                            _tree.LeafNode($"[{i++}] {e.ExpeditionId} {e.RarePouchId} {e.NameId}");
                        }
                    }
                    foreach (var m in _tree.Node("Expedition Items", agent->Data->ExpeditionItems.Size() == 0))
                    {
                        int i = 0;
                        foreach (var e in agent->Data->ExpeditionItems.Span)
                        {
                            _tree.LeafNode($"[{i++}] {e.ExpeditionId} {e.PouchId} {e.u5}");
                        }
                    }
                    foreach (var m in _tree.Node("Resources", agent->Data->Resources.Size() == 0))
                    {
                        int i = 0;
                        foreach (var e in agent->Data->Resources.Span)
                        {
                            _tree.LeafNode($"[{i++}] {e.PouchId} {e.ItemId} {e.IconId}");
                        }
                    }
                    foreach (var m in _tree.Node("Pending icon updates", agent->Data->ItemsPendingIconUpdate.Size() == 0))
                    {
                        int i = 0;
                        foreach (var e in agent->Data->ItemsPendingIconUpdate.Span)
                        {
                            _tree.LeafNode($"[{i++}] {e}");
                        }
                    }
                }
            }
        }

        _tree.LeafNode($"Cowries: {Utils.NumCowries()} (enough for {GranaryUtils.MaxDays()} days)");
    }

    private unsafe void DrawGranaryState(int i, ref MJIGranaryState state)
    {
        var expedition = state.RemainingDays > 0 ? $"{state.ActiveExpeditionId} ({Service.LuminaRow<MJIName>(state.ActiveExpeditionId + 1u)!.Singular}) {state.RemainingDays}days" : "none";
        foreach (var n in _tree.Node($"G{i}: {expedition}, finish-at={DateTimeOffset.FromUnixTimeSeconds(state.FinishTime)}"))
        {
            DrawGranaryItem("rare", state.RareResourcePouchId, state.RareResourceCount);
            for (int k = 0; k < 20; ++k)
                DrawGranaryItem(k.ToString(), state.NormalResourcePouchIds[k], state.NormalResourceCounts[k]);
        }
    }

    private unsafe void DrawGranaryItem(string prompt, uint mjiPouchId, int count)
    {
        if (count <= 0)
            return;
        var item = Service.LuminaRow<MJIItemPouch>(mjiPouchId)!.Item;
        var avail = Utils.NumItems(item.Row);
        _tree.LeafNode($"[{prompt}] {mjiPouchId} {item.Value!.Name}: {avail}+{count}");
    }
}
