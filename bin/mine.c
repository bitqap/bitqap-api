// example.c
//
// gcc mine.c -lssl -lcrypto -o mine.o

#include <openssl/evp.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>



void bytes2md5(const char *data, int len, char *md5buf) {
  // Based on https://www.openssl.org/docs/manmaster/man3/EVP_DigestUpdate.html
  EVP_MD_CTX *mdctx = EVP_MD_CTX_new();
  const EVP_MD *md = EVP_md5();
  unsigned char md_value[EVP_MAX_MD_SIZE];
  unsigned int md_len, i;
  EVP_DigestInit_ex(mdctx, md, NULL);
  EVP_DigestUpdate(mdctx, data, len);
  EVP_DigestFinal_ex(mdctx, md_value, &md_len);
  EVP_MD_CTX_free(mdctx);
  for (i = 0; i < md_len; i++) {
    snprintf(&(md5buf[i * 2]), 16 * 2, "%02x", md_value[i]);
  }
}

void substring(char s[], char sub[], int p, int l) {
   int c = 0;  
   while (c < l) {
      sub[c] = s[p+c-1];
      c++;
   }
   sub[c] = '\0';
}

int main(int argc, char *argv[]) {
        // gcc untitled.c -lssl -lcrypto -o untitled.o
        // converting this part of SHELL script as much as I
        // while [ $ZEROS != $DIFFZEROS ]
        //        do
        //                       # increase the nonce by one
        //                        let "NONCE += 1"
        //                        # do the hashing
        //                        HASH=$(printf "`cat $CURRENTBLOCK.wip`\n\n## Nonce: #################################################################################\n$NONCE\n" | md5sum)
        //                        HASH=$(echo $HASH | cut -d" " -f1)
        //                        # print the hash to the screen because it looks cool
        //                        #echo $HASH
        //                        # cut the leading zeros off the hash
        //                        ZEROS=$(echo $HASH | cut -c1-$DIFF)
        //        done

        int NONCE=0 ;
        char nonce[5];
        char md5[33];

        char * ZEROS=argv[2];
        char * DIFFZEROS=argv[3];
		printf("Length of string DIFFZEROS = %zu \n",strlen(DIFFZEROS));
        
        int DIFF = strtol(argv[4], NULL, 10);

        // File read into buffer
        char buffer[300000];   // Buffer to store data
        FILE * stream;
        stream = fopen(argv[1], "r");
        int count = fread(&buffer, sizeof(char), 300000, stream);
        fclose(stream);
        char BUFFER[300000];
        
        while (1) {
                NONCE++ ;
                strcpy(BUFFER,buffer);    // file content read once in above. Always reconstruct BUFFER (raw file content)
                strcat(BUFFER,"\n\n## Nonce: #################################################################################\n");
                sprintf(nonce, "%d\n", NONCE);                    // convert int NONCE to nonce as string
                strcat(BUFFER,nonce);                             // concatinate 
                bytes2md5(BUFFER, strlen(BUFFER), md5);           // calculate md5sum
                substring(md5, ZEROS, 1, DIFF);                   // substring defined zeros
                
                // https://stackoverflow.com/questions/8004237/how-do-i-properly-compare-strings-in-c
                if (strcmp(ZEROS,DIFFZEROS) == 0) {
                        printf(md5); printf("  "); printf(nonce); // if HASH found print HASH and NONCE
                        printf("\n");
                        break ; 
                }        
                // EMPTY vars
                memset(BUFFER,0,sizeof(BUFFER));
                memset(md5,0,sizeof(md5));
                memset(nonce,0,sizeof(nonce));
        }
        return 0;
}