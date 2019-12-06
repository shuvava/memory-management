///
/// to build run g++ manual_mem_mng.cpp -o ../../build/manual_mem_mng
///
///
#include <iostream>
#include <string>

void printReport(int* data) {
    std::cout << "Report: " << *data << "\n";
}

int main() {
    try {
        int* ptr;
        ptr = new int();
        *ptr = 25;
        printReport(ptr);
        delete ptr;
        ptr = 0;
        return 0;
    }
    catch (std::bad_alloc& ba)
    {
        std::cout << "ERROR: Out of memory\n";
    return 1;
    }
}
