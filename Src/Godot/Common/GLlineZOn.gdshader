shader_type spatial;

render_mode blend_mix, cull_disabled, depth_test_disabled ,depth_draw_never;

void vertex() {
    // No custom vertex logic needed
}

void fragment() {
    // No custom fragment logic needed
    // The shader will use the default behavior, which is to render the material's albedo color
}
