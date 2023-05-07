#version 330 core
out vec4 FragColor;
in vec2 TexCoord;
uniform sampler2D world;
uniform sampler2D light;
uniform float BloomSize = 5.0;


vec4 blm(vec2 uv)
{
    const float Pi = 6.28318530718; // Pi*2
    
    // GAUSSIAN BLUR SETTINGS {{{
    const float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
    const float Quality = 4.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
   


    vec2 Radius = vec2(1,1) * (BloomSize / 600.0);
    
    // Pixel colour
    vec4 Color = vec4(0,0,0,0);//100,149,237

    // Blur calculations
    for( float d=0.0; d<Pi; d+=Pi/Directions)
    {
		for(float i=1.0/Quality; i<=1.0; i+=1.0/Quality)
        {
			Color += texture(light, uv+vec2(cos(d * 2),sin(d))*Radius*i);		
        }
    }
        // Output to screen
    //if(Color.a > 0)
    {
        Color /= Directions * 2;
        Color.a = (Color.r / 3 + Color.g / 3 + Color.b / 3) * 1.2;
    }

  	return Color + texture(world, uv);
}

void main()
{FragColor = blm(TexCoord);
}