shader_type particles;
render_mode keep_data, disable_force, disable_velocity;

void process() {
	float amount = LIFETIME;

	// top and bottom world-space edge points of this node
	vec4 a = EMISSION_TRANSFORM * vec4(0, 1, 0, 1);
	vec4 b = EMISSION_TRANSFORM * vec4(0, -1, 0, 1);

	// first-ever init: spread particles across the circular buffer
	if (CUSTOM.w == 0.0) {
		CUSTOM.w = float(INDEX) + 1.0;
		CUSTOM.z = amount; // pass total amount to draw pass
		TRANSFORM = mat4(a, a, b, b);
	}

	// wrap circular buffer
	if (CUSTOM.w == amount + 1.0) {
		CUSTOM.w = 1.0;
	}

	// slot 1: cache this frame's positions (not yet visible as a segment)
	if (CUSTOM.w == 1.0) {
		TRANSFORM = mat4(a, a, b, b);
	}

	// slot 2: set right edge of the quad using previous frame's left edge
	if (CUSTOM.w == 2.0) {
		TRANSFORM[1] = a;
		TRANSFORM[2] = b;
	}

	CUSTOM.w++;
}
