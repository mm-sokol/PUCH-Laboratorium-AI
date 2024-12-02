import os
import base64

# Define the folder containing images and the output file
image_folder = "../screens"
output_folder = "../screensBase64"

# Create or overwrite the output file

for filename in os.listdir(image_folder):
    if filename.endswith((".png", ".jpg", ".jpeg", ".gif")):  # Add more extensions if needed
        image_path = os.path.join(image_folder, filename)
        out_file = os.path.join(output_folder, f"{os.path.splitext(filename)[0]}.txt")
        with open(out_file, "w") as out_file:
            with open(image_path, "rb") as img_file:
                # Read and encode the image to Base64
                base64_string = base64.b64encode(img_file.read()).decode("utf-8")
                # Write the filename and Base64 string to the output file
                out_file.write(base64_string)
                print(f"Encoded {filename}")
            

