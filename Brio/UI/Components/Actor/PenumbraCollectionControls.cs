﻿using Brio.Game.Actor;
using Brio.Game.Actor.Extensions;
using Brio.IPC;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ImGuiNET;

namespace Brio.UI.Components;

public static class PenumbraCollectionControls
{
    private static string? _selectedCollection = null;

    public unsafe static void Draw(GameObject gameObject)
    {
        if(PenumbraIPCService.Instance.IsPenumbraEnabled)
        {
            var collections = PenumbraCollectionService.Instance.Collections;

            if(_selectedCollection == null && collections.Count > 0)
                _selectedCollection = collections[0];

            ImGui.SetNextItemWidth(ImGui.GetFontSize() * 8);
            ImGui.BeginGroup();
            if(ImGui.BeginCombo("Collection", _selectedCollection))
            {
                foreach(var collection in collections)
                {
                    bool selected = collection == _selectedCollection;
                    if(ImGui.Selectable(collection, selected))
                    {
                        _selectedCollection = collection;
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.EndGroup();

            ImGui.SameLine();

            ImGui.PushFont(UiBuilder.IconFont);
            if(ImGui.Button(FontAwesomeIcon.Redo.ToIconString()))
            {
                PenumbraCollectionService.Instance.RefreshCollections();
            }
            ImGui.PopFont();

            bool isCharacter = gameObject.AsNative()->IsCharacter();
            bool allowed = isCharacter && PenumbraCollectionService.Instance.CanApplyCollection(gameObject);
            if(!allowed) ImGui.BeginDisabled();
            if(ImGui.Button("Apply Collection"))
            {
                PenumbraCollectionService.Instance.RedrawActorWithCollection(gameObject, _selectedCollection!);
            }
            if(!isCharacter) ImGui.Text("Must be a character type.");
            if(!allowed) ImGui.EndDisabled();
        }
        else
        {
            ImGui.TextColored(new(1, 0, 0, 1), "Penumbra integration not active");
            ImGui.TextColored(new(1, 0, 0, 1), "See settings");
        }
    }
}
