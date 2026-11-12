@tool
extends EditorScenePostImport
## Optional import script for beaverford_village.glb (Import dock → Import Script).
## Godot 4.7's glTF importer does not switch on "Vertex Color → Use as Albedo" for the FIRST surface of a mesh,
## even when that surface has vertex colours (COLOR_0). On the Terrain this is M_Road, which carries the subtle
## road / square tint in its vertex colours. This script enables the flag on every surface that has vertex colours.

func _post_import(scene: Node) -> Object:
	_fix(scene)
	return scene

func _fix(n: Node) -> void:
	if n is MeshInstance3D and n.mesh:
		for s in n.mesh.get_surface_count():
			var arrays: Array = n.mesh.surface_get_arrays(s)
			var mat: Material = n.mesh.surface_get_material(s)
			if arrays[Mesh.ARRAY_COLOR] != null and mat is BaseMaterial3D:
				(mat as BaseMaterial3D).vertex_color_use_as_albedo = true
	for c in n.get_children():
		_fix(c)
