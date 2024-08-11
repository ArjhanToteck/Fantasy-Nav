shader_type canvas_item;

uniform sampler2D flowMap; //Displacement map
uniform float strength = 0.1;    //Force of the effect


void fragment(){
	vec4 offset = texture(flowMap, vec2(UV.x, UV.y)) * strength; //Get offset 
	//COLOR = texture(TEXTURE, vec2(UV.x,UV.y) + normal.xy); //Apply offset
	COLOR += texture(TEXTURE, vec2(UV.x,UV.y) + offset.xy - vec2(0.5,0.5)*strength); //We need to remove the displacement 
}