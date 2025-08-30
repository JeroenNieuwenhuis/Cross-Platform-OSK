import math
import argparse
import sys
import os

# The factor to multiply numbers by
SCALE_FACTOR = 0.8

def process_line(line_content):
    """
    Finds and scales all numbers in a single line of text.
    This is the core logic, now applied to smaller chunks (lines).
    """
    output_parts = []
    i = 0
    content_len = len(line_content)

    while i < content_len:
        char = line_content[i]

        is_start_of_number = char.isdigit() or \
                             (char == '-' and i + 1 < content_len and line_content[i+1].isdigit())
        is_preceded_by_hash = (i > 0 and line_content[i-1] == '#')

        if is_start_of_number and not is_preceded_by_hash:
            j = i + 1
            dot_found = False
            while j < content_len:
                next_char = line_content[j]
                if next_char.isdigit():
                    j += 1
                elif next_char == '.' and not dot_found:
                    dot_found = True
                    j += 1
                else:
                    break
            
            num_str = line_content[i:j]
            try:
                value = float(num_str)
                scaled_value = value * SCALE_FACTOR
                result = int(math.floor(scaled_value))
                output_parts.append(str(result))
                i = j
            except ValueError:
                output_parts.append(num_str)
                i = j
        else:
            output_parts.append(char)
            i += 1

    return "".join(output_parts)

def process_file_stream(input_path, output_stream):
    """
    Processes the input file line-by-line and writes to an output stream.
    Includes a progress indicator.
    """
    # First, count lines for the progress indicator without loading the file
    print("Pre-calculating file size for progress bar...")
    try:
        with open(input_path, 'r', encoding='utf-8', errors='ignore') as f:
            total_lines = sum(1 for _ in f)
    except Exception as e:
        print(f"Could not count lines in file: {e}", file=sys.stderr)
        total_lines = 0 # Fallback

    print(f"Found {total_lines} lines. Starting processing...")

    with open(input_path, 'r', encoding='utf-8', errors='ignore') as f_in:
        for i, line in enumerate(f_in):
            processed_line = process_line(line)
            output_stream.write(processed_line)

            # Update progress indicator periodically
            if (i + 1) % 50 == 0 or (i + 1) == total_lines:
                percent_complete = ((i + 1) / total_lines) * 100
                # \r moves cursor to beginning of line, end='' prevents newline
                print(f"\rProcessing: Line {i+1}/{total_lines} ({percent_complete:.1f}%)", end="")
    
    print("\nProcessing complete.")


def main():
    """Main function to run the script from the command line."""
    parser = argparse.ArgumentParser(
        description="""Multiply all integers and floats in a text file by 0.8
                       and round down. Skips hexadecimal values.
                       Optimized for large files.""",
        formatter_class=argparse.RawTextHelpFormatter
    )
    parser.add_argument(
        'input_file',
        help='The path to the input text file.'
    )
    parser.add_argument(
        '-o', '--output',
        dest='output_file',
        help='Optional. The path to the output file. If not provided, prints to console.'
    )

    args = parser.parse_args()

    if not os.path.exists(args.input_file):
        print(f"Error: Input file not found at '{args.input_file}'", file=sys.stderr)
        sys.exit(1)

    if args.output_file:
        try:
            with open(args.output_file, 'w', encoding='utf-8') as f_out:
                process_file_stream(args.input_file, f_out)
            print(f"Successfully saved result to '{args.output_file}'")
        except Exception as e:
            print(f"\nError writing to output file: {e}", file=sys.stderr)
            sys.exit(1)
    else:
        # If no output file, use standard output as the stream
        process_file_stream(args.input_file, sys.stdout)


if __name__ == '__main__':
    main()