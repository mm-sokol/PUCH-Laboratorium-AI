import os
import re
import base64
import argparse

def convert_image_to_base64(image_path):
    """Convert an image to a base64 string."""
    with open(image_path, "rb") as image_file:
        base64_data = base64.b64encode(image_file.read()).decode("utf-8")
    return f"data:image/png;base64,{base64_data}"

def process_markdown_file(md_file_path, out_path):
    """Process the Markdown file and replace image paths with Base64 encoded data."""
    with open(md_file_path, "r", encoding="utf-8") as file:
        content = file.read()

    # Regex to match ![alt text](path/to/image.png)
    markdown_image_filenames_pattern = re.compile(r'!\[[a-zA-Z\-\.\w]*?\]\((screens/[a-zA-Z\-\. ]*?)\)')
    markdown_image_pattern = re.compile(r'!\[([a-zA-Z\-\.\w]*?)\]\((screens/[a-zA-Z\-\. ]*?)\)')
    # Regex to match <img src="path/to/image.png" ... />
    html_image_filenames_pattern = re.compile(r'<img src="(screens/.*?)" alt=".*?" width="\d*?"\s*\/>')
    html_image_pattern = re.compile(r'<img src="(screens\/.*?)" alt="(.*?)" width="(\d*?)"\s*\/>')

    # Find all matches for Markdown-style images
    markdown_matches = markdown_image_filenames_pattern.findall(content)
    # Find all matches for HTML-style images
    html_matches = html_image_filenames_pattern.findall(content)

    # Combine matches and remove duplicates
    all_matches = list(set(markdown_matches + html_matches))

    for image_path in all_matches:
        path = os.path.join("..", image_path)
        if os.path.exists(path):
            print(f"Processing: {image_path}")
            base64_data = convert_image_to_base64(path)
            # Replace Markdown-style paths
            content = markdown_image_pattern.sub(
                lambda m: f'![{m.group(1)}]({base64_data})', content
            )
            # Replace HTML-style paths
            content = html_image_pattern.sub(
                lambda m: f'<img src="{base64_data}" alt="{m.group(1)}" width="{m.group(3)}"/>', content
            )
        else:
            print(f"Warning: Image not found at path {image_path}")

    # Save the updated Markdown file
    with open(out_path, "w", encoding="utf-8") as file:
        file.write(content)
    print(f"Processed file saved: {out_path}")

# Example usage
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Replace image paths with Base64 encoded data in a Markdown file.")
    parser.add_argument("markdown_file", help="Path to the Markdown file to process.")
    parser.add_argument("output_file", help="Path to the output file.")
    args = parser.parse_args()

    # Call the function with the file provided as an argument
    if os.path.exists(args.markdown_file):
        process_markdown_file(args.markdown_file, args.output_file)
    else:
        print(f"Error: File {args.markdown_file} does not exist.")
